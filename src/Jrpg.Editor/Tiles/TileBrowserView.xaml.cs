using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Shows the <see cref="TileBrowserViewModel"/>'s sheet, scaled so its cells match the map's, with the cells that are
/// tiles marked; a click chooses the whole cell under it.
/// </summary>
internal sealed partial class TileBrowserView : UserControl
{
    private TileBrowserViewModel? _browser;
    private SheetCutViewModel? _cut;

    public TileBrowserView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    /// <summary>Where the sheets' images come from; set by the window, which shares it with the map drawing.</summary>
    public TileImages? Images { get; set; }

    /// <summary>While it marks blocked tiles, a click flips the cell instead of choosing it; set by the window.</summary>
    public WalkabilityMarking? Marking { get; set; }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _browser?.PropertyChanged -= OnBrowserChanged;
        _browser = DataContext as TileBrowserViewModel;
        _browser?.PropertyChanged += OnBrowserChanged;
        DrawSheet();
    }

    private void OnBrowserChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TileBrowserViewModel.Cut))
        {
            _cut?.PropertyChanged -= OnCutChanged;
            _cut = _browser?.Cut;
            _cut?.PropertyChanged += OnCutChanged;
        }

        if (e.PropertyName is nameof(TileBrowserViewModel.SelectedChoice) or nameof(TileBrowserViewModel.SelectedCell) or nameof(TileBrowserViewModel.SheetTiles) or nameof(TileBrowserViewModel.Cut))
        {
            DrawSheet();
        }
    }

    private void OnCutChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SheetCutViewModel.PreviewSheet))
        {
            DrawSheet();
        }
    }

    /// <summary>Draws the shown image again; the window calls it when an import replaced its file.</summary>
    public void RedrawSheet() => DrawSheet();

    // An image not set up has no cells to scale to the map's, so it shows at its own size, with no marks, or with the
    // Cut panel's grid while it is open; the scale stays put while the cut is typed.
    private void DrawSheet()
    {
        if (_browser?.SelectedChoice is not { } choice || Images is null)
        {
            SheetImage.Source = SheetMarks.Source = null;
            return;
        }

        var image = Images.SheetImageOf(choice.File);
        var size = new Size(image.PixelWidth, image.PixelHeight);
        var marked = _browser.Cut is { } cut ? cut.PreviewSheet : choice.Sheet;
        SheetImage.Source = image;
        SheetMarks.Source = marked is null ? null : TilesheetMarks.Draw(marked, size, _browser.SheetTiles, _browser.SelectedCell);
        SheetImage.Width = SheetMarks.Width = size.Width;
        SheetImage.Height = SheetMarks.Height = size.Height;
        var scale = choice.Sheet is { } sheet ? TilesheetMarks.DisplayScaleOf(sheet) : 1;
        SheetView.LayoutTransform = new ScaleTransform(scale, scale);
    }

    private void OnSheetClicked(object sender, MouseButtonEventArgs e)
    {
        var pixel = e.GetPosition(SheetView);
        if (Marking is { IsMarkingBlocked: true })
        {
            Marking.FlipCellAt(pixel.X, pixel.Y);
        }
        else
        {
            _browser?.SelectCellAt(pixel.X, pixel.Y);
        }
    }
}
