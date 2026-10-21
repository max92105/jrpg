using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Choosing the tile to paint: one tilesheet at a time, where a click on a cell that is a tile selects it. Cells that
/// aren't tiles yet can't be painted; making tiles from them comes with the tilesheet tools.
/// </summary>
internal sealed partial class TileBrowserViewModel : ObservableObject
{
    private readonly CatalogDraft _catalog;

    public TileBrowserViewModel(CatalogDraft catalog)
    {
        _catalog = catalog;
        Sheets = [.. catalog.Current.Tilesheets.Values.OrderBy(sheet => sheet.Id, StringComparer.Ordinal)];
        SelectedSheet = Sheets.Count > 0 ? Sheets[0] : null;
    }

    public IReadOnlyList<TilesheetDefinition> Sheets { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SheetTiles))]
    public partial TilesheetDefinition? SelectedSheet { get; set; }

    /// <summary>The tiles cut from the shown sheet.</summary>
    public IReadOnlyList<TileDefinition> SheetTiles =>
        SelectedSheet is null ? [] : [.. _catalog.Current.Tiles.Values.Where(tile => tile.SheetId == SelectedSheet.Id)];

    /// <summary>The tile the brush paints; null until one is chosen.</summary>
    [ObservableProperty]
    public partial int? SelectedTileId { get; private set; }

    /// <summary>Selects the tile at a point of the shown sheet's image; a cell that isn't a tile keeps the selection.</summary>
    /// <param name="x">In the image's own pixels, from its left edge.</param>
    /// <param name="y">In the image's own pixels, from its top edge.</param>
    public void SelectTileAt(double x, double y)
    {
        if (SelectedSheet is not { } sheet)
        {
            return;
        }

        var column = (int)Math.Floor((x - sheet.Margin) / (sheet.TileWidth + sheet.Spacing));
        var row = (int)Math.Floor((y - sheet.Margin) / (sheet.TileHeight + sheet.Spacing));
        if (SheetTiles.FirstOrDefault(tile => tile.Column == column && tile.Row == row) is { } picked)
        {
            SelectedTileId = picked.Id;
        }
    }

    /// <summary>Selects a tile and shows its sheet, as picking a tile from the map does.</summary>
    public void Select(int tileId)
    {
        SelectedSheet = Sheets.First(sheet => sheet.Id == _catalog.Current.Tiles[tileId].SheetId);
        SelectedTileId = tileId;
    }
}
