using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// The Tiles panel's Mark blocked toggle (decision 119, the only place walkability is set): while it is on, a click on a
/// sheet cell flips its tile between walkable and blocked instead of choosing it.
/// </summary>
/// <param name="catalog">Where the tiles are changed.</param>
/// <param name="browser">The shown sheet, which finds the cell under a click.</param>
internal sealed partial class WalkabilityMarking(CatalogDraft catalog, TileBrowserViewModel browser) : ObservableObject
{
    [ObservableProperty]
    public partial bool IsMarkingBlocked { get; set; }

    /// <summary>Flips the tile of the cell under an image pixel of the shown sheet; a cell that isn't a tile yet becomes a blocked one.</summary>
    /// <param name="x">In the image's own pixels, from its left edge.</param>
    /// <param name="y">In the image's own pixels, from its top edge.</param>
    public void FlipCellAt(double x, double y)
    {
        if (browser.CellAt(x, y) is not { } cell)
        {
            return;
        }

        if (catalog.TileAt(cell) is { } tile)
        {
            catalog.SetWalkable(tile.Id, !tile.IsWalkable);
        }
        else
        {
            catalog.SetWalkable(catalog.AddTile(cell), isWalkable: false);
        }
    }
}
