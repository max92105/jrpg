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
        if (e.PropertyName is nameof(TileBrowserViewModel.SelectedChoice) or nameof(TileBrowserViewModel.SelectedCell) or nameof(TileBrowserViewModel.SheetTiles))
        {
            DrawSheet();
        }
    }

    // An image not set up has no cells to scale to the map's, so it shows at its own size, with no marks.
    private void DrawSheet()
    {
        if (_browser?.SelectedChoice is not { } choice || Images is null)
        {
            SheetImage.Source = SheetMarks.Source = null;
            return;
        }

        var image = Images.SheetImageOf(choice.File);
        var size = new Size(image.PixelWidth, image.PixelHeight);
        SheetImage.Source = image;
        SheetMarks.Source = choice.Sheet is { } sheet ? TilesheetMarks.Draw(sheet, size, _browser.SheetTiles, _browser.SelectedCell) : null;
        SheetImage.Width = SheetMarks.Width = size.Width;
        SheetImage.Height = SheetMarks.Height = size.Height;
        var scale = choice.Sheet is { } cut ? TilesheetMarks.DisplayScaleOf(cut) : 1;
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
