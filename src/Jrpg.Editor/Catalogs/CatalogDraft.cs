using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The content the editor works on (ADR 0014): as loaded, with the maps saved since. Every panel and the map drawing read
/// <see cref="Current"/> when they need it, so a change made here reaches them all; only this type changes it. Each
/// catalog change is an undo step, announced by <see cref="StepRecorded"/> so the shell can order it among map steps.
/// </summary>
internal sealed partial class CatalogDraft : ObservableObject
{
    private readonly CatalogHistory _history = new();

    /// <param name="loaded">The content as the loader validated it.</param>
    public CatalogDraft(ContentCatalog loaded) => Current = loaded;

    /// <summary>A catalog change became an undo step.</summary>
    public event EventHandler? StepRecorded;

    /// <summary>A change outside the history (a tile made by painting) dropped the undone steps, which can't be redone now.</summary>
    public event EventHandler? RedoDropped;

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
    /// Makes a sheet cell a tile, walkable (decision 114), with the id one above the highest; tiles are never renumbered.
    /// Not an undo step (decision 113): undoing the paint that made it keeps it.
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

    /// <summary>The tiles the next undo would remove, so it can be refused while one is in use; empty when it removes none.</summary>
    public IReadOnlyList<int> TilesRemovedByUndo() =>
        _history.NextUndo is { } edit ? [.. Current.Tiles.Keys.Except(edit.Revert(Current).Tiles.Keys).Order()] : [];

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

    private TileDefinition NewTileAt(SheetCell cell, bool isWalkable) => new()
    {
        Id = Current.Tiles.Keys.Append(TileLayer.EmptyTileId).Max() + 1,
        IsWalkable = isWalkable,
        SheetId = cell.SheetId,
        Column = cell.Column,
        Row = cell.Row,
    };

    private void Do(CatalogEdit edit)
    {
        Current = edit.Apply(Current);
        _history.Push(edit);
        OnPropertyChanged(nameof(IsDirty));
        StepRecorded?.Invoke(this, EventArgs.Empty);
    }
}
