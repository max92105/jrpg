using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// The Animate panel (decision 467): the frames to the right and the time typed for the cell chosen in the Tiles panel,
/// what stops Apply, and the preview's cell at a time (ADR 0039).
/// </summary>
internal sealed partial class TileAnimationViewModel : ObservableObject
{
    /// <summary>What the panel offers for a tile with no animation yet. Placeholder.</summary>
    public const int DefaultFrameCount = 4;

    /// <summary>What the panel offers for a tile with no animation yet, in milliseconds. Placeholder.</summary>
    public const int DefaultFrameMilliseconds = 150;

    private readonly CatalogDraft _catalog;
    private readonly TileBrowserViewModel _tiles;
    private readonly SheetImages _images;

    /// <param name="catalog">The tiles and sheets.</param>
    /// <param name="tiles">Whose chosen cell the panel opens on.</param>
    /// <param name="images">The sheets' image sizes, for how many columns a sheet has.</param>
    public TileAnimationViewModel(CatalogDraft catalog, TileBrowserViewModel tiles, SheetImages images)
    {
        (_catalog, _tiles, _images) = (catalog, tiles, images);
        tiles.PropertyChanged += OnTilesChanged;
        catalog.PropertyChanged += OnCatalogChanged;
    }

    /// <summary>A cell is chosen on a set-up sheet whose image is there, a tile or not yet.</summary>
    public bool CanOpen =>
        _tiles.SelectedCell is { } cell && _tiles.SheetChoices.Any(choice => choice.Sheet?.Id == cell.SheetId && !choice.IsImageMissing);

    /// <summary>The cell the panel opened on, its animation's first frame; null while closed.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOpen), nameof(Problem))]
    public partial SheetCell? Cell { get; private set; }

    public bool IsOpen => Cell is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string FrameCountText { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Problem))]
    public partial string FrameMillisecondsText { get; set; } = "";

    /// <summary>What stops Apply; null when the typed animation fits the sheet.</summary>
    public string? Problem => Cell is { } cell ? ProblemOf(cell) : null;

    /// <summary>
    /// The cell shown <paramref name="elapsed"/> into the typed animation; the opened cell while a problem stands.
    /// </summary>
    /// <exception cref="InvalidOperationException">The panel is closed.</exception>
    public SheetCell PreviewCellAt(TimeSpan elapsed)
    {
        var cell = Cell ?? throw new InvalidOperationException("The Animate panel is closed.");
        return TypedAnimation() is { } animation ? cell with { Column = cell.Column + animation.FrameAt(elapsed) } : cell;
    }

    [RelayCommand(CanExecute = nameof(CanOpen))]
    private void Open()
    {
        var cell = _tiles.SelectedCell!.Value;
        var animation = _catalog.TileAt(cell)?.Animation;
        FrameCountText = (animation?.FrameCount ?? DefaultFrameCount).ToString(CultureInfo.InvariantCulture);
        FrameMillisecondsText = (animation?.FrameMilliseconds ?? DefaultFrameMilliseconds).ToString(CultureInfo.InvariantCulture);
        Cell = cell;
    }

    [RelayCommand]
    private void Close() => Cell = null;

    // The typed animation; null while a problem stands.
    private TileAnimation? TypedAnimation() =>
        Problem is null && WholeNumberOf(FrameCountText) is { } frames && WholeNumberOf(FrameMillisecondsText) is { } milliseconds
            ? new TileAnimation { FrameCount = frames, FrameMilliseconds = milliseconds }
            : null;

    private static int? WholeNumberOf(string text) =>
        int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var number) ? number : null;

    private string? ProblemOf(SheetCell cell)
    {
        if (WholeNumberOf(FrameCountText) is not { } frames || frames < TileAnimation.MinFrames)
        {
            return $"Frames must be a whole number from {TileAnimation.MinFrames}.";
        }

        if (WholeNumberOf(FrameMillisecondsText) is not { } milliseconds || milliseconds < 1)
        {
            return "The time must be a whole number of milliseconds from 1.";
        }

        var sheet = _catalog.Current.Tilesheets[cell.SheetId];
        var columns = sheet.ColumnsIn(_images.SizeOf(sheet.File).Width);
        // Subtracted, not added: a typed count near the int limit would wrap the sum and pass.
        return frames > columns - cell.Column ? $"{frames} frames from column {cell.Column} run past the sheet's {columns} columns." : null;
    }

    // An undone Set up or a removed sheet takes the cell's sheet away: the panel closes rather than read it.
    private void OnCatalogChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogDraft.Current) && Cell is { } cell && !_catalog.Current.Tilesheets.ContainsKey(cell.SheetId))
        {
            Cell = null;
        }
    }

    private void OnTilesChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TileBrowserViewModel.SelectedCell) or nameof(TileBrowserViewModel.SheetChoices))
        {
            OnPropertyChanged(nameof(CanOpen));
            OpenCommand.NotifyCanExecuteChanged();
        }
    }
}
