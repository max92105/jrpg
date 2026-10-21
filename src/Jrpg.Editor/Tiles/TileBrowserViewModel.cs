using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Choosing what to paint with: one tilesheet image at a time, where a click on a set-up sheet chooses the whole cell
/// under it, a tile or not. A cell that isn't a tile yet becomes one when painted.
/// </summary>
internal sealed partial class TileBrowserViewModel : ObservableObject
{
    private readonly CatalogDraft _catalog;
    private readonly IReadOnlyList<string> _imageFiles;
    private readonly Func<string, (int Width, int Height)> _imageSizeOf;
    // The sheets the choices were built from; a catalog change replaces this dictionary only when it changes a sheet.
    private IReadOnlyDictionary<string, TilesheetDefinition> _listedSheets;

    /// <param name="catalog">The sheets and tiles.</param>
    /// <param name="imageFiles">Every image in the tilesheet folder, in the order the sheet box lists them.</param>
    /// <param name="imageSizeOf">A sheet image's size in pixels, by its file name, so the browser reads no files.</param>
    public TileBrowserViewModel(CatalogDraft catalog, IReadOnlyList<string> imageFiles, Func<string, (int Width, int Height)> imageSizeOf)
    {
        _catalog = catalog;
        _imageFiles = imageFiles;
        _imageSizeOf = imageSizeOf;
        catalog.PropertyChanged += OnCatalogChanged;
        _listedSheets = catalog.Current.Tilesheets;
        SheetChoices = ChoicesOf(imageFiles, _listedSheets.Values);
        SelectedChoice = SheetChoices.Count > 0 ? SheetChoices[0] : null;
    }

    /// <summary>Every image, each set-up sheet by its id; an image cut into two sheets is listed twice.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<SheetChoice> SheetChoices { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedSheet), nameof(SheetTiles), nameof(ChoiceText), nameof(CanSetUpSheet))]
    [NotifyCanExecuteChangedFor(nameof(SetUpSheetCommand))]
    public partial SheetChoice? SelectedChoice { get; set; }

    /// <summary>How the shown image is cut; null when it isn't set up yet.</summary>
    public TilesheetDefinition? SelectedSheet => SelectedChoice?.Sheet;

    /// <summary>Whether the shown image has no sheet yet, so Set up can make one.</summary>
    public bool CanSetUpSheet => SelectedChoice is { Sheet: null };

    /// <summary>The tiles cut from the shown sheet.</summary>
    public IReadOnlyList<TileDefinition> SheetTiles =>
        SelectedSheet is null ? [] : [.. _catalog.Current.Tiles.Values.Where(tile => tile.SheetId == SelectedSheet.Id)];

    /// <summary>The chosen cell, on whichever sheet it is; null until one is chosen.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTileId), nameof(ChoiceText))]
    public partial SheetCell? SelectedCell { get; private set; }

    /// <summary>The tile the brush paints: the chosen cell's tile, or null when it isn't a tile yet.</summary>
    public int? SelectedTileId => SelectedCell is { } cell ? _catalog.TileAt(cell)?.Id : null;

    /// <summary>
    /// The line under the sheet: the chosen cell and its tile, or that it isn't one yet. On an image not set up, it says so
    /// and names the cell the brush still paints.
    /// </summary>
    public string ChoiceText => (SelectedChoice, SelectedCell) switch
    {
        ({ Sheet: null }, null) => "Not set up yet",
        ({ Sheet: null }, { } cell) => $"Not set up yet. Brush: cell ({cell.Column}, {cell.Row}) of {cell.SheetId}",
        (_, null) => "",
        (_, { } cell) when _catalog.TileAt(cell) is { } tile => $"Cell ({cell.Column}, {cell.Row}): tile {tile.Id}",
        (_, { } cell) => $"Cell ({cell.Column}, {cell.Row}): not a tile yet",
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
        SelectedChoice = SheetChoices.First(choice => choice.Sheet?.Id == tile.SheetId);
        SelectedCell = new SheetCell(tile.SheetId, tile.Column, tile.Row);
    }

    /// <summary>
    /// Makes the shown image a sheet, as one undo step: id from its file name (decision 118), square tiles of the guessed
    /// size, no margin or spacing.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSetUpSheet))]
    private void SetUpSheet()
    {
        var file = SelectedChoice!.File;
        var (width, height) = _imageSizeOf(file);
        var tileSize = SheetCutGuess.TileSizeFor(width, height);
        _catalog.SetTilesheet(new TilesheetDefinition
        {
            Id = TilesheetIds.FromFile(file, [.. _catalog.Current.Tilesheets.Keys]),
            File = file,
            TileWidth = tileSize,
            TileHeight = tileSize,
            Margin = 0,
            Spacing = 0,
        });
    }

    // Matched ignoring case, as Windows finds the file a sheet names whatever its case.
    private static SheetChoice[] ChoicesOf(IReadOnlyList<string> imageFiles, IEnumerable<TilesheetDefinition> sheets)
    {
        var sheetsByFile = sheets.OrderBy(sheet => sheet.Id, StringComparer.Ordinal).ToLookup(sheet => sheet.File, StringComparer.OrdinalIgnoreCase);
        return [.. imageFiles.SelectMany(file => sheetsByFile.Contains(file)
            ? sheetsByFile[file].Select(sheet => new SheetChoice(sheet.File, sheet))
            : [new SheetChoice(file, null)])];
    }

    // A tile made by painting is marked on the sheet, and named under it, at once; a sheet set up or undone is listed anew.
    private void OnCatalogChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CatalogDraft.Current))
        {
            return;
        }

        if (!ReferenceEquals(_catalog.Current.Tilesheets, _listedSheets))
        {
            ListSheetsAnew();
        }

        OnPropertyChanged(nameof(SheetTiles));
        OnPropertyChanged(nameof(SelectedTileId));
        OnPropertyChanged(nameof(ChoiceText));
    }

    // The same image stays shown, by its sheet if that is still there, else by its file. A chosen cell whose sheet is gone
    // is let go, so painting never makes a tile on a sheet that doesn't exist.
    private void ListSheetsAnew()
    {
        var shown = SelectedChoice;
        _listedSheets = _catalog.Current.Tilesheets;
        SheetChoices = ChoicesOf(_imageFiles, _listedSheets.Values);
        SelectedChoice = SheetChoices.FirstOrDefault(choice => choice.Sheet is { } sheet && sheet.Id == shown?.Sheet?.Id)
            ?? SheetChoices.FirstOrDefault(choice => string.Equals(choice.File, shown?.File, StringComparison.OrdinalIgnoreCase));
        if (SelectedCell is { } cell && !_listedSheets.ContainsKey(cell.SheetId))
        {
            SelectedCell = null;
        }
    }
}
