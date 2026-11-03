using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The content the editor works on (ADR 0014): as loaded, with the maps saved since. Every panel and the map drawing read
/// <see cref="Current"/> when they need it, so a change made here reaches them all; only this type changes it. Each
/// catalog change is an undo step, announced by <see cref="StepRecorded"/> so the shell can order it among map steps.
/// </summary>
internal sealed partial class CatalogDraft : ObservableObject
{
    private readonly CatalogHistory _history = new();

    // The highest tile id seen this session: loaded, or made here. A removed sheet's tiles may come back by undo, so a
    // new tile never takes an id below it (design key decision 3).
    private int _highestTileId;

    /// <param name="loaded">The content as the loader validated it.</param>
    public CatalogDraft(ContentCatalog loaded)
    {
        Current = loaded;
        _highestTileId = loaded.Tiles.Keys.Append(TileLayer.EmptyTileId).Max();
    }

    /// <summary>A catalog change became an undo step.</summary>
    public event EventHandler? StepRecorded;

    /// <summary>A change outside the history (a tile made by painting) dropped the undone steps, which can't be redone now.</summary>
    public event EventHandler? RedoDropped;

    /// <summary>
    /// The content was written straight away and is about to replace <see cref="Current"/> (the sheet purge), given as
    /// the argument: the open map and every history start afresh. Raised before <see cref="Current"/> changes, so the
    /// open map is replaced first and nothing ever draws an old layout against the new catalogs (it would name tiles
    /// and objects that are gone); the new layout is a subset of the old, so drawing it against the old catalogs is
    /// safe.
    /// </summary>
    public event EventHandler<ContentCatalog>? Rewriting;

    /// <summary>An immutable snapshot, replaced whole by each change, as <c>MapViewModel.Map</c> is for maps.</summary>
    [ObservableProperty]
    public partial ContentCatalog Current { get; private set; }

    /// <summary>Whether a tile, sheet or object differs from the last save; undoing back to it makes the catalogs clean again.</summary>
    public bool IsDirty => _history.IsDirty;

    public bool CanUndo => _history.CanUndo;

    public bool CanRedo => _history.CanRedo;

    /// <summary>The tile cut from a sheet cell, or null when the cell isn't a tile yet.</summary>
    public TileDefinition? TileAt(SheetCell cell) =>
        // Two tiles may share a cell (nothing in the content forbids it); the lowest id wins.
        Current.Tiles.Values
            .Where(tile => tile.SheetId == cell.SheetId && tile.Column == cell.Column && tile.Row == cell.Row)
            .MinBy(tile => tile.Id);

    /// <summary>
    /// Makes a sheet cell a tile, walkable (decision 114), with the id one above the highest seen this session; tiles
    /// are never renumbered and an id is never reused. Not an undo step (decision 113): undoing the paint that made it
    /// keeps it.
    /// </summary>
    /// <returns>The new tile's id.</returns>
    public int AddTile(SheetCell cell)
    {
        var tile = NewTileAt(cell, isWalkable: true);
        var hadRedo = _history.CanRedo;
        Current = WithTile(Current, tile);
        _history.RecordChangeOutsideHistory();
        OnPropertyChanged(nameof(IsDirty));
        if (hadRedo)
        {
            // A paint off the map makes the tile but records no map step, so the shell hears of the lost redo here.
            RedoDropped?.Invoke(this, EventArgs.Empty);
        }

        return tile.Id;
    }

    /// <summary>
    /// Each cell's tile, the cells not tiles yet made walkable tiles first (as <see cref="AddTile"/>, not undo steps):
    /// what an object built from the cells needs.
    /// </summary>
    public IReadOnlyDictionary<SheetCell, int> TileIdsOf(IReadOnlyCollection<SheetCell> cells) =>
        cells.Distinct().ToDictionary(cell => cell, cell => TileAt(cell)?.Id ?? AddTile(cell));

    /// <summary>Makes a sheet cell a blocked tile, as one undo step: undoing it removes the tile.</summary>
    public void AddBlockedTile(SheetCell cell)
    {
        var tile = NewTileAt(cell, isWalkable: false);
        Do(new CatalogEdit(catalog => WithTile(catalog, tile), catalog => WithoutTile(catalog, tile.Id)));
    }

    /// <summary>Whether the player can walk onto the tile (Core's walkability rule); nothing changes when it already is.</summary>
    public void SetWalkable(int tileId, bool isWalkable)
    {
        var before = Current.Tiles[tileId];
        if (before.IsWalkable != isWalkable)
        {
            var after = before with { IsWalkable = isWalkable };
            Do(new CatalogEdit(catalog => WithTile(catalog, after), catalog => WithTile(catalog, before)));
        }
    }

    /// <summary>Adds a sheet, or replaces the one with its id, as one undo step.</summary>
    public void SetTilesheet(TilesheetDefinition sheet)
    {
        var before = Current.Tilesheets.GetValueOrDefault(sheet.Id);
        Do(new CatalogEdit(
            catalog => WithTilesheet(catalog, sheet),
            catalog => before is null ? WithoutTilesheet(catalog, sheet.Id) : WithTilesheet(catalog, before)));
    }

    /// <summary>
    /// Replaces the object with its id, or adds it when the id is new, as one undo step; nothing when the record is the same.
    /// </summary>
    public void SetObject(ObjectDefinition definition)
    {
        var before = Current.Objects.GetValueOrDefault(definition.Id);
        if (before is not null && IsSameObject(before, definition))
        {
            return;
        }

        Do(new CatalogEdit(
            catalog => WithObject(catalog, definition),
            catalog => before is null ? WithoutObject(catalog, definition.Id) : WithObject(catalog, before)));
    }

    /// <summary>
    /// Takes a sheet and every tile cut from it out as one undo step (decision 443); the caller has checked nothing
    /// uses the tiles.
    /// </summary>
    /// <exception cref="InvalidOperationException">No sheet has the id.</exception>
    public void RemoveTilesheet(string sheetId)
    {
        var sheet = Current.Tilesheets.GetValueOrDefault(sheetId) ?? throw new InvalidOperationException($"No sheet '{sheetId}' to remove.");
        var tiles = Current.Tiles.Values.Where(tile => tile.SheetId == sheetId).ToList();
        Do(new CatalogEdit(
            catalog => WithoutTilesheet(catalog, sheetId) with { Tiles = catalog.Tiles.Where(pair => pair.Value.SheetId != sheetId).ToDictionary() },
            catalog => WithTilesheet(catalog, sheet) with { Tiles = catalog.Tiles.Values.Concat(tiles).ToDictionary(tile => tile.Id) }));
    }

    /// <summary>
    /// Takes an object definition out as one undo step (decision 443); the caller has checked nothing uses it.
    /// </summary>
    /// <exception cref="InvalidOperationException">No object has the id.</exception>
    public void RemoveObject(string objectId)
    {
        var before = Current.Objects.GetValueOrDefault(objectId) ?? throw new InvalidOperationException($"No object '{objectId}' to remove.");
        Do(new CatalogEdit(catalog => WithoutObject(catalog, objectId), catalog => WithObject(catalog, before)));
    }

    /// <summary>
    /// Replaces the quests as one undo step (decision 394); nothing when they are the same by content, stages included.
    /// </summary>
    public void SetQuests(IReadOnlyList<QuestDefinition> quests)
    {
        var before = Current.Quests;
        var after = quests.ToDictionary(quest => quest.Id);
        if (before.Count == after.Count && after.All(pair => before.TryGetValue(pair.Key, out var quest) && quest == pair.Value))
        {
            return;
        }

        Do(new CatalogEdit(catalog => catalog with { Quests = after }, catalog => catalog with { Quests = before }));
    }

    /// <summary>The tiles the next undo would remove, so it can be refused while one is in use; empty when it removes none.</summary>
    public IReadOnlyList<int> TilesRemovedByUndo() =>
        _history.NextUndo is { } edit ? [.. Current.Tiles.Keys.Except(edit.Revert(Current).Tiles.Keys).Order()] : [];

    /// <summary>The objects the next undo would remove, so it can be refused while a map places one.</summary>
    public IReadOnlyList<string> ObjectsRemovedByUndo() =>
        _history.NextUndo is { } edit ? [.. Current.Objects.Keys.Except(edit.Revert(Current).Objects.Keys).Order(StringComparer.Ordinal)] : [];

    /// <summary>The sheets the next undo would remove, so it can be refused while tiles are cut from one.</summary>
    public IReadOnlyList<string> SheetsRemovedByUndo() =>
        _history.NextUndo is { } edit ? [.. Current.Tilesheets.Keys.Except(edit.Revert(Current).Tilesheets.Keys).Order(StringComparer.Ordinal)] : [];

    /// <summary>The quests the next undo would remove, so it can be refused while a map's event names one.</summary>
    public IReadOnlyList<string> QuestsRemovedByUndo() =>
        _history.NextUndo is { } edit ? [.. Current.Quests.Keys.Except(edit.Revert(Current).Quests.Keys).Order(StringComparer.Ordinal)] : [];

    /// <summary>
    /// The stages the next undo would take from quests it keeps, so it can be refused while a map's event names one.
    /// </summary>
    public IReadOnlyList<(string QuestId, string StageId)> QuestStagesRemovedByUndo()
    {
        if (_history.NextUndo is not { } edit)
        {
            return [];
        }

        var reverted = edit.Revert(Current).Quests;
        return [.. Current.Quests.Values
            .Where(quest => reverted.ContainsKey(quest.Id))
            .SelectMany(quest => quest.Stages.Where(stage => !reverted[quest.Id].HasStage(stage.Id)).Select(stage => (quest.Id, stage.Id)))];
    }

    /// <summary>Takes back the newest catalog step; nothing when there is none.</summary>
    public void Undo()
    {
        if (_history.CanUndo)
        {
            Current = _history.Undo().Revert(Current);
            OnPropertyChanged(nameof(IsDirty));
        }
    }

    /// <summary>Does the newest undone catalog step again; nothing when there is none.</summary>
    public void Redo()
    {
        if (_history.CanRedo)
        {
            Current = _history.Redo().Apply(Current);
            OnPropertyChanged(nameof(IsDirty));
        }
    }

    /// <summary>
    /// Takes content already written as the current one, with nothing to undo and nothing unsaved (decision 442's
    /// purge, which can't be undone); the tile id mark keeps its value, so no id is reused.
    /// </summary>
    public void ReplaceAfterRewrite(ContentCatalog written)
    {
        Rewriting?.Invoke(this, written);
        _history.StartAfresh();
        Current = written;
        OnPropertyChanged(nameof(IsDirty));
    }

    public void MarkSaved()
    {
        _history.MarkSaved();
        OnPropertyChanged(nameof(IsDirty));
    }

    /// <summary>The saved map replaces its older version, or joins the maps when it is new, so later saves check against it.</summary>
    public void AddSavedMap(MapDefinition map) =>
        Current = Current with { Maps = new Dictionary<string, MapDefinition>(Current.Maps) { [map.Id] = map } };

    private static ContentCatalog WithTile(ContentCatalog catalog, TileDefinition tile) =>
        catalog with { Tiles = new Dictionary<int, TileDefinition>(catalog.Tiles) { [tile.Id] = tile } };

    private static ContentCatalog WithoutTile(ContentCatalog catalog, int tileId) =>
        catalog with { Tiles = catalog.Tiles.Where(pair => pair.Key != tileId).ToDictionary() };

    private static ContentCatalog WithObject(ContentCatalog catalog, ObjectDefinition definition) =>
        catalog with { Objects = new Dictionary<string, ObjectDefinition>(catalog.Objects) { [definition.Id] = definition } };

    private static ContentCatalog WithoutObject(ContentCatalog catalog, string objectId) =>
        catalog with { Objects = catalog.Objects.Where(pair => pair.Key != objectId).ToDictionary() };

    // Records compare lists by reference, so an outline edited back to the same points would otherwise count as a change:
    // the with-copy compares every field but the lists, and the lists are compared by content below.
    private static bool IsSameObject(ObjectDefinition before, ObjectDefinition after) =>
        before with { CollisionPolygon = after.CollisionPolygon, Tiles = after.Tiles, AbovePlayerZones = after.AbovePlayerZones } == after
        && before.CollisionPolygon.SequenceEqual(after.CollisionPolygon)
        && before.Tiles.SequenceEqual(after.Tiles)
        && before.AbovePlayerZones.Count == after.AbovePlayerZones.Count
        && before.AbovePlayerZones.Zip(after.AbovePlayerZones).All(pair => pair.First.SequenceEqual(pair.Second));

    private static ContentCatalog WithTilesheet(ContentCatalog catalog, TilesheetDefinition sheet) =>
        catalog with { Tilesheets = new Dictionary<string, TilesheetDefinition>(catalog.Tilesheets) { [sheet.Id] = sheet } };

    private static ContentCatalog WithoutTilesheet(ContentCatalog catalog, string sheetId) =>
        catalog with { Tilesheets = catalog.Tilesheets.Where(pair => pair.Key != sheetId).ToDictionary() };

    private TileDefinition NewTileAt(SheetCell cell, bool isWalkable)
    {
        _highestTileId++;
        return new()
        {
            Id = _highestTileId,
            IsWalkable = isWalkable,
            SheetId = cell.SheetId,
            Column = cell.Column,
            Row = cell.Row,
        };
    }


    private void Do(CatalogEdit edit)
    {
        Current = edit.Apply(Current);
        _history.Push(edit);
        OnPropertyChanged(nameof(IsDirty));
        StepRecorded?.Invoke(this, EventArgs.Empty);
    }
}
