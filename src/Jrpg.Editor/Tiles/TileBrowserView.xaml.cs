using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// Shows the <see cref="TileBrowserViewModel"/>'s sheet, its cells at the zoom's size, with the cells that are tiles
/// marked; a click chooses the whole cell under it, Ctrl + wheel zooms toward the mouse.
/// </summary>
internal sealed partial class TileBrowserView : UserControl
{
    private TileBrowserViewModel? _browser;

    // Where a press on the sheet started: in image pixels for the cells, and in the view's own units for the drag
    // distance; null when no press is under way.
    private (Point Pixel, Point OnView)? _press;
    private bool _isDraggingCells;
    private SheetCutViewModel? _cut;
    private CellZoom? _zoom;
    private double _scale = 1;

    // Where Ctrl + wheel left the mouse, which the next zoom keeps in place; the viewer's center for the bar's buttons.
    private Point? _zoomAnchor;

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
        // An open Cut panel is let go with its browser: the object composer's picker drops its browser on close.
        _cut?.PropertyChanged -= OnCutChanged;
        _cut = _browser?.Cut;
        _cut?.PropertyChanged += OnCutChanged;
        _zoom?.PropertyChanged -= OnZoomChanged;
        _zoom = _browser?.Zoom;
        _zoom?.PropertyChanged += OnZoomChanged;
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

        if (e.PropertyName is nameof(TileBrowserViewModel.SelectedChoice) or nameof(TileBrowserViewModel.MarkedCells) or nameof(TileBrowserViewModel.SheetTiles) or nameof(TileBrowserViewModel.Cut))
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

    private void OnZoomChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(CellZoom.CellPixels))
        {
            return;
        }

        var oldScale = _scale;
        DrawSheet();
        var anchor = _zoomAnchor ?? new Point(SheetScroller.ViewportWidth / 2, SheetScroller.ViewportHeight / 2);
        _zoomAnchor = null;
        ZoomScrolling.KeepAnchor(SheetScroller, anchor, oldScale, _scale);
    }

    // The plain wheel scrolls as before; a command at its bound doesn't run (the toolkit's Execute doesn't check).
    private void OnSheetMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || _zoom is null)
        {
            return;
        }

        e.Handled = true;
        var command = e.Delta > 0 ? _zoom.ZoomInCommand : _zoom.ZoomOutCommand;
        if (command.CanExecute(null))
        {
            _zoomAnchor = e.GetPosition(SheetScroller);
            command.Execute(null);
        }
    }

    /// <summary>Draws the shown image again; the window calls it when an import replaced its file.</summary>
    public void RedrawSheet() => DrawSheet();

    // An image not set up has no cells to scale, so it shows at the zoom over the map's cell size, with no marks, or
    // with the Cut panel's grid while it is open; the scale stays put while the cut is typed.
    private void DrawSheet()
    {
        if (_browser?.SelectedChoice is not { IsImageMissing: false } choice || Images is null)
        {
            SheetImage.Source = SheetMarks.Source = null;
            return;
        }

        var image = Images.SheetImageOf(choice.File);
        var size = new Size(image.PixelWidth, image.PixelHeight);
        var marked = _browser.Cut is { } cut ? cut.PreviewSheet : choice.Sheet;
        SheetImage.Source = image;
        _scale = TilesheetMarks.ShownScaleOf(choice.Sheet, _browser.Zoom);
        SheetMarks.Source = marked is null ? null : TilesheetMarks.Draw(new ShownSheet(marked, size, _scale), _browser.SheetTiles, _browser.MarkedCells);
        SheetImage.Width = SheetMarks.Width = size.Width;
        SheetImage.Height = SheetMarks.Height = size.Height;
        SheetView.LayoutTransform = new ScaleTransform(_scale, _scale);
    }

    private void OnSheetClicked(object sender, MouseButtonEventArgs e)
    {
        var pixel = e.GetPosition(SheetView);
        if (Marking is { IsMarkingBlocked: true })
        {
            Marking.FlipCellAt(pixel.X, pixel.Y);
            return;
        }

        // A click chooses the cell under it; dragging on from it (with or without Shift) chooses a rectangle of cells.
        _browser?.SelectCellAt(pixel.X, pixel.Y);
        _press = (pixel, e.GetPosition(this));
        SheetView.CaptureMouse();
    }

    // Once the mouse has moved past the system's drag distance, the rectangle follows it until the button is released.
    private void OnSheetMouseMoved(object sender, MouseEventArgs e)
    {
        if (_press is not { } press)
        {
            return;
        }

        var moved = e.GetPosition(this) - press.OnView;
        _isDraggingCells |= Math.Abs(moved.X) >= SystemParameters.MinimumHorizontalDragDistance
            || Math.Abs(moved.Y) >= SystemParameters.MinimumVerticalDragDistance;
        if (_isDraggingCells)
        {
            var at = e.GetPosition(SheetView);
            _browser?.SelectCellsBetween(new Point2D(press.Pixel.X, press.Pixel.Y), new Point2D(at.X, at.Y));
        }
    }

    private void OnSheetReleased(object sender, MouseButtonEventArgs e) => SheetView.ReleaseMouseCapture();

    private void OnSheetLostMouseCapture(object sender, MouseEventArgs e)
    {
        _press = null;
        _isDraggingCells = false;
    }
}
