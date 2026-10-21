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

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _browser?.PropertyChanged -= OnBrowserChanged;
        _browser = DataContext as TileBrowserViewModel;
        _browser?.PropertyChanged += OnBrowserChanged;
        DrawSheet();
    }

    private void OnBrowserChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TileBrowserViewModel.SelectedSheet) or nameof(TileBrowserViewModel.SelectedCell))
        {
            DrawSheet();
        }
    }

    private void DrawSheet()
    {
        if (_browser?.SelectedSheet is not { } sheet || Images is null)
        {
            SheetImage.Source = SheetMarks.Source = null;
            return;
        }

        var image = Images.SheetImageOf(sheet.File);
        var size = new Size(image.PixelWidth, image.PixelHeight);
        SheetImage.Source = image;
        SheetMarks.Source = TilesheetMarks.Draw(sheet, size, _browser.SheetTiles, _browser.SelectedCell);
        SheetImage.Width = SheetMarks.Width = size.Width;
        SheetImage.Height = SheetMarks.Height = size.Height;
        var scale = TilesheetMarks.DisplayScaleOf(sheet);
        SheetView.LayoutTransform = new ScaleTransform(scale, scale);
    }

    private void OnSheetClicked(object sender, MouseButtonEventArgs e)
    {
        var pixel = e.GetPosition(SheetView);
        _browser?.SelectCellAt(pixel.X, pixel.Y);
    }
}
