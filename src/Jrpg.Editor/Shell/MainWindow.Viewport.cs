using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The map view's pan and zoom: the wheel, the arrows while the map has focus, Ctrl+0 and Ctrl+1, Fit and Actual size,
/// and the transform, grid and marks they redraw; the camera's rules are in <see cref="ViewportTransform"/>.
/// </summary>
internal sealed partial class MainWindow
{
    // Placeholder steps: 15 % per wheel notch, 150 screen pixels per arrow press.
    private const double WheelZoomStep = 1.15;
    private const double ArrowPanStep = 150;

    private Point? _panFrom;
    private (double Zoom, int Width, int Height)? _gridDrawnFor;

    // A newly opened map is fitted to the view, which has no size until the window is laid out.
    private bool _isFitPending = true;

    private bool HandleZoomKey(Key key)
    {
        switch (key)
        {
            case Key.D0 or Key.NumPad0:
                RefitMap();
                return true;
            case Key.D1 or Key.NumPad1:
                ZoomAtViewCenter(1);
                return true;
            default:
                return false;
        }
    }

    // Only while the map has focus, so arrows still move through the map and layer lists.
    private bool HandleViewportKey(Key key)
    {
        if (!MapViewport.IsKeyboardFocusWithin)
        {
            return false;
        }

        var pan = key switch
        {
            Key.Left => new Vector(ArrowPanStep, 0),
            Key.Right => new Vector(-ArrowPanStep, 0),
            Key.Up => new Vector(0, ArrowPanStep),
            Key.Down => new Vector(0, -ArrowPanStep),
            _ => (Vector?)null,
        };
        if (pan is { } delta)
        {
            _viewport.PanBy(delta);
            ApplyViewport();
            return true;
        }

        return key == Key.Space;
    }

    private void OnFitClicked(object sender, RoutedEventArgs e) => RefitMap();

    private void OnActualSizeClicked(object sender, RoutedEventArgs e) => ZoomAtViewCenter(1);

    private void OnViewportSizeChanged(object sender, SizeChangedEventArgs e) => FitMap();

    private void StartPanning(Point from)
    {
        _panFrom = from;
        MapViewport.CaptureMouse();
        MapViewport.Cursor = Cursors.SizeAll;
    }

    private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var zoom = e.Delta > 0 ? _viewport.Zoom * WheelZoomStep : _viewport.Zoom / WheelZoomStep;
        _viewport.ZoomAt(e.GetPosition(MapViewport), zoom);
        ApplyViewport();
        e.Handled = true;
    }

    private GridCell CellAt(MouseEventArgs e) => _viewport.ScreenToCell(e.GetPosition(MapViewport), MapDrawing.CellPixels);

    private Point2D MapPointAt(MouseEventArgs e)
    {
        var mapPixel = _viewport.ScreenToMap(e.GetPosition(MapViewport));
        return new Point2D(mapPixel.X / MapDrawing.CellPixels, mapPixel.Y / MapDrawing.CellPixels);
    }

    private void RefitMap()
    {
        _isFitPending = true;
        FitMap();
    }

    private void FitMap()
    {
        var layout = _viewModel.CurrentMap?.Map.Layout;
        if (!_isFitPending || layout is null || MapViewport.ActualWidth <= 0 || MapViewport.ActualHeight <= 0)
        {
            return;
        }

        var mapSize = new Size(layout.Width * MapDrawing.CellPixels, layout.Height * MapDrawing.CellPixels);
        _viewport.Fit(mapSize, new Size(MapViewport.ActualWidth, MapViewport.ActualHeight));
        _isFitPending = false;
        ApplyViewport();
    }

    private void ZoomAtViewCenter(double zoom)
    {
        _viewport.ZoomAt(new Point(MapViewport.ActualWidth / 2, MapViewport.ActualHeight / 2), zoom);
        ApplyViewport();
    }

    private void ApplyViewport()
    {
        var zoom = _viewport.Zoom;
        MapLayers.RenderTransform = new MatrixTransform(zoom, 0, 0, zoom, _viewport.Origin.X, _viewport.Origin.Y);
        ZoomStatus.Content = $"{zoom:P0}";

        // The grid's lines are one screen pixel wide, so they're redrawn when the zoom or the map's size changes, not
        // on every pan.
        if (_viewModel.CurrentMap?.Map.Layout is not { } layout || _gridDrawnFor == (zoom, layout.Width, layout.Height))
        {
            return;
        }

        GridImage.Source = MapDrawing.DrawGrid(layout, 1 / zoom);
        DrawCollision();
        DrawSelectionMarks();

        // The marks are redrawn in place; a drag under way puts them back under the dragged objects at the new zoom.
        ShowObjectDragAt(_mapPoint);
        _gridDrawnFor = (zoom, layout.Width, layout.Height);
    }
}
