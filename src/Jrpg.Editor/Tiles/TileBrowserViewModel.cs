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
    private readonly SheetImages _images;
    private readonly Func<string, bool> _confirm;
    private readonly Func<MapLayout?> _openLayout;
    // The sheets the choices were built from; a catalog change replaces this dictionary only when it changes a sheet.
    private IReadOnlyDictionary<string, TilesheetDefinition> _listedSheets;

    /// <param name="catalog">The sheets and tiles.</param>
    /// <param name="images">The tilesheet folder's images.</param>
    /// <param name="confirm">Asks the developer a yes or no question, before a re-cut changes tiles in use.</param>
    /// <param name="openLayout">The open map's layout, unsaved changes included, or null; for what a re-cut touches.</param>
    public TileBrowserViewModel(CatalogDraft catalog, SheetImages images, Func<string, bool> confirm, Func<MapLayout?> openLayout)
    {
        _catalog = catalog;
        _images = images;
        _confirm = confirm;
        _openLayout = openLayout;
        catalog.PropertyChanged += OnCatalogChanged;
        _listedSheets = catalog.Current.Tilesheets;
        SheetChoices = ChoicesOf(images.Files, _listedSheets.Values);
        SelectedChoice = SheetChoices.Count > 0 ? SheetChoices[0] : null;
    }

    /// <summary>Every image, each set-up sheet by its id; an image cut into two sheets is listed twice.</summary>
    [ObservableProperty]
    public partial IReadOnlyList<SheetChoice> SheetChoices { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedSheet), nameof(SheetTiles), nameof(ChoiceText), nameof(CanSetUpSheet), nameof(CanCutSheet))]
    [NotifyCanExecuteChangedFor(nameof(SetUpSheetCommand), nameof(CutSheetCommand))]
    public partial SheetChoice? SelectedChoice { get; set; }

    /// <summary>How the shown image is cut; null when it isn't set up yet.</summary>
    public TilesheetDefinition? SelectedSheet => SelectedChoice?.Sheet;

    /// <summary>Whether the shown image has no sheet yet and no Cut panel open, so Set up can start one.</summary>
    public bool CanSetUpSheet => SelectedChoice is { Sheet: null } && Cut is null;

    /// <summary>Whether the shown image is a sheet and no Cut panel is open, so Cut can re-cut it.</summary>
    public bool CanCutSheet => SelectedChoice is { Sheet: not null } && Cut is null;

    /// <summary>The open Cut panel, for the shown image; null when closed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCutOpen), nameof(CanSetUpSheet), nameof(CanCutSheet))]
    [NotifyCanExecuteChangedFor(nameof(ApplyCutCommand), nameof(SetUpSheetCommand), nameof(CutSheetCommand))]
    public partial SheetCutViewModel? Cut { get; private set; }

    public bool IsCutOpen => Cut is not null;

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

        var (width, height) = _images.SizeOf(sheet.File);
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
    /// Opens the Cut panel for the shown image, pre-filled with a guess: id from its file name (decision 118), square tiles
    /// of the guessed size, no margin or spacing. The sheet is made only when the cut is applied.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSetUpSheet))]
    private void SetUpSheet()
    {
        var file = SelectedChoice!.File;
        var imageSize = _images.SizeOf(file);
        var tileSize = SheetCutGuess.TileSizeFor(imageSize.Width, imageSize.Height);
        var guess = new TilesheetDefinition
        {
            Id = TilesheetIds.FromFile(file, [.. _catalog.Current.Tilesheets.Keys]),
            File = file,
            TileWidth = tileSize,
            TileHeight = tileSize,
            Margin = 0,
            Spacing = 0,
        };
        Cut = new SheetCutViewModel(guess, imageSize, SheetUsage.None);
    }

    /// <summary>Opens the Cut panel on the shown sheet's cut; applying it re-cuts the sheet, its tiles keeping their cells.</summary>
    [RelayCommand(CanExecute = nameof(CanCutSheet))]
    private void CutSheet()
    {
        var sheet = SelectedSheet!;
        Cut = new SheetCutViewModel(sheet, _images.SizeOf(sheet.File), CatalogUsage.OfSheet(_catalog.Current, sheet.Id, _openLayout()));
    }

    /// <summary>
    /// Makes the sheet as the Cut panel has it, as one undo step, and closes the panel. A re-cut of a sheet with tiles asks
    /// first with the panel's warning (decision 116), counted again here as the open map may have changed since the panel
    /// opened; on no, the panel stays open.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanApplyCut))]
    private void ApplyCut()
    {
        var cut = Cut!;
        cut.Usage = CatalogUsage.OfSheet(_catalog.Current, cut.SheetId, _openLayout());
        if (cut.Problem is not null || (cut.Warning is { } warning && !_confirm($"{warning} Re-cut it?")))
        {
            return;
        }

        Cut = null;
        _catalog.SetTilesheet(cut.ToSheet());
    }

    private bool CanApplyCut() => Cut is { Problem: null };

    [RelayCommand]
    private void CancelCut() => Cut = null;

    partial void OnCutChanged(SheetCutViewModel? oldValue, SheetCutViewModel? newValue)
    {
        oldValue?.PropertyChanged -= OnCutFieldChanged;
        newValue?.PropertyChanged += OnCutFieldChanged;
    }

    private void OnCutFieldChanged(object? sender, PropertyChangedEventArgs e) => ApplyCutCommand.NotifyCanExecuteChanged();

    // A cut belongs to the image it was opened on.
    partial void OnSelectedChoiceChanged(SheetChoice? value) => Cut = null;

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

        if (Cut is { } cut)
        {
            cut.Usage = CatalogUsage.OfSheet(_catalog.Current, cut.SheetId, _openLayout());
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
        SheetChoices = ChoicesOf(_images.Files, _listedSheets.Values);
        SelectedChoice = SheetChoices.FirstOrDefault(choice => choice.Sheet is { } sheet && sheet.Id == shown?.Sheet?.Id)
            ?? SheetChoices.FirstOrDefault(choice => string.Equals(choice.File, shown?.File, StringComparison.OrdinalIgnoreCase));
        if (SelectedCell is { } cell && !_listedSheets.ContainsKey(cell.SheetId))
        {
            SelectedCell = null;
        }
    }
}
