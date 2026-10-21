using CommunityToolkit.Mvvm.ComponentModel;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Choosing what to paint with: one tilesheet at a time, where a click chooses the whole cell under it, a tile or not.
/// A cell that isn't a tile yet doesn't paint until painting makes tiles (plan 2026-10-21-editor, E3d).
/// </summary>
internal sealed partial class TileBrowserViewModel : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly Func<string, (int Width, int Height)> _imageSizeOf;

    /// <param name="catalog">The sheets and tiles.</param>
    /// <param name="imageSizeOf">A sheet image's size in pixels, by its file name, so the browser reads no files.</param>
    public TileBrowserViewModel(CatalogDraft catalog, Func<string, (int Width, int Height)> imageSizeOf)
    {
        _catalog = catalog;
        _imageSizeOf = imageSizeOf;
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

    /// <summary>The chosen cell, on whichever sheet it is; null until one is chosen.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTileId), nameof(ChoiceText))]
    public partial SheetCell? SelectedCell { get; private set; }

    /// <summary>The tile the brush paints: the chosen cell's tile, or null when it isn't a tile yet.</summary>
    public int? SelectedTileId => SelectedCell is { } cell ? TileAt(cell)?.Id : null;

    /// <summary>The line under the sheet: the chosen cell and its tile, or that it isn't one yet.</summary>
    public string ChoiceText => SelectedCell switch
    {
        null => "",
        { } cell when TileAt(cell) is { } tile => $"Cell ({cell.Column}, {cell.Row}): tile {tile.Id}",
        { } cell => $"Cell ({cell.Column}, {cell.Row}): not a tile yet",
    };

    /// <summary>The whole cell of the shown sheet under an image pixel; null on the margin, on spacing or past the last whole cell.</summary>
    /// <param name="x">In the image's own pixels, from its left edge.</param>
    /// <param name="y">In the image's own pixels, from its top edge.</param>
    public SheetCell? CellAt(double x, double y)
    {
        if (SelectedSheet is not { } sheet)
        {
            return null;
        }

        var (width, height) = _imageSizeOf(sheet.File);
        var column = (int)Math.Floor((x - sheet.Margin) / (sheet.TileWidth + sheet.Spacing));
        var row = (int)Math.Floor((y - sheet.Margin) / (sheet.TileHeight + sheet.Spacing));
        var isInGrid = column >= 0 && row >= 0 && column < sheet.ColumnsIn(width) && row < sheet.RowsIn(height);
        var isInCell = x < sheet.CellLeft(column) + sheet.TileWidth && y < sheet.CellTop(row) + sheet.TileHeight;
        return isInGrid && isInCell ? new SheetCell(sheet.Id, column, row) : null;
    }

    /// <summary>Chooses the cell under an image pixel of the shown sheet; a pixel off every whole cell keeps the choice.</summary>
    public void SelectCellAt(double x, double y)
    {
        if (CellAt(x, y) is { } cell)
        {
            SelectedCell = cell;
        }
    }

    /// <summary>Chooses a tile's cell and shows its sheet, as picking a tile from the map does.</summary>
    public void Select(int tileId)
    {
        var tile = _catalog.Current.Tiles[tileId];
        SelectedSheet = Sheets.First(sheet => sheet.Id == tile.SheetId);
        SelectedCell = new SheetCell(tile.SheetId, tile.Column, tile.Row);
    }

    // Two tiles may share a cell (nothing in the content forbids it); the lowest id wins.
    private TileDefinition? TileAt(SheetCell cell) =>
        _catalog.Current.Tiles.Values
            .Where(tile => tile.SheetId == cell.SheetId && tile.Column == cell.Column && tile.Row == cell.Row)
            .MinBy(tile => tile.Id);
}
