using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The content the editor works on (ADR 0014): as loaded, with the maps saved since. Every panel and the map drawing read
/// <see cref="Current"/> when they need it, so a change made here reaches them all; only this type changes it.
/// </summary>
internal sealed partial class CatalogDraft : ObservableObject
{
    /// <param name="loaded">The content as the loader validated it.</param>
    public CatalogDraft(ContentCatalog loaded) => Current = loaded;

    /// <summary>An immutable snapshot, replaced whole by each change, as <c>MapViewModel.Map</c> is for maps.</summary>
    [ObservableProperty]
    public partial ContentCatalog Current { get; private set; }

    /// <summary>Whether a tile, sheet or object changed since the catalogs were last saved.</summary>
    public bool IsDirty { get; private set; }

    /// <summary>The tile cut from a sheet cell, or null when the cell isn't a tile yet.</summary>
    public TileDefinition? TileAt(SheetCell cell) =>
        // Two tiles may share a cell (nothing in the content forbids it); the lowest id wins.
        Current.Tiles.Values
            .Where(tile => tile.SheetId == cell.SheetId && tile.Column == cell.Column && tile.Row == cell.Row)
            .MinBy(tile => tile.Id);

    /// <summary>Makes a sheet cell a tile, walkable (decision 114), with the id one above the highest; tiles are never renumbered.</summary>
    /// <returns>The new tile's id.</returns>
    public int AddTile(SheetCell cell)
    {
        var id = Current.Tiles.Keys.Append(TileLayer.EmptyTileId).Max() + 1;
        var tile = new TileDefinition { Id = id, IsWalkable = true, SheetId = cell.SheetId, Column = cell.Column, Row = cell.Row };
        IsDirty = true;
        Current = Current with { Tiles = new Dictionary<int, TileDefinition>(Current.Tiles) { [id] = tile } };
        return id;
    }

    public void MarkSaved() => IsDirty = false;

    /// <summary>The saved map replaces its older version, or joins the maps when it is new, so later saves check against it.</summary>
    public void AddSavedMap(MapDefinition map) =>
        Current = Current with { Maps = new Dictionary<string, MapDefinition>(Current.Maps) { [map.Id] = map } };
}
