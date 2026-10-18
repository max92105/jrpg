using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The editor window. Turning the open map into WPF images and mouse and keys into pan and zoom are view concerns, done
/// here; the view models hold plain data and run in tests without a UI thread (ADR 0012). The camera rules are in
/// <see cref="ViewportTransform"/>.
/// </summary>
internal sealed partial class MainWindow : Window
{
    // Legacy MapEditor steps: 15 % per wheel notch, 150 screen pixels per arrow press.
    private const double WheelZoomStep = 1.15;
    private const double ArrowPanStep = 150;

    private readonly MainViewModel _viewModel;
    private readonly MapDrawing? _drawing;
    private readonly ViewportTransform _viewport = new();
    private bool _isSpaceHeld;
    private Point? _panFrom;
    private double? _gridZoom;

    // A newly opened map is fitted to the view, which has no size until the window is laid out.
    private bool _isFitPending = true;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;
        _drawing = viewModel.Catalog is { } catalog ? new MapDrawing(catalog, viewModel.ArtRoot) : null;
        viewModel.PropertyChanged += OnViewModelChanged;
        ShowCurrentMap();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        e.Handled = Keyboard.Modifiers == ModifierKeys.Control ? HandleZoomKey(e.Key) : HandleViewportKey(e.Key);
    }

    protected override void OnPreviewKeyUp(KeyEventArgs e)
    {
        base.OnPreviewKeyUp(e);
        if (e.Key == Key.Space)
        {
            _isSpaceHeld = false;
        }
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.CurrentMap))
        {
            ShowCurrentMap();
        }
    }

    private void ShowCurrentMap()
    {
        var layout = _viewModel.CurrentMap?.Map.Layout;
        MapImage.Source = layout is not null && _drawing is not null ? _drawing.Draw(layout) : null;
        _gridZoom = null;
        RefitMap();
    }

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

        _isSpaceHeld |= key == Key.Space;
        return key == Key.Space;
    }

    private void OnFitClicked(object sender, RoutedEventArgs e) => RefitMap();

    private void OnActualSizeClicked(object sender, RoutedEventArgs e) => ZoomAtViewCenter(1);

    private void OnViewportSizeChanged(object sender, SizeChangedEventArgs e) => FitMap();

    private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
    {
        MapViewport.Focus();
        var isPanGesture = e.ChangedButton == MouseButton.Middle || (e.ChangedButton == MouseButton.Left && _isSpaceHeld);
        if (isPanGesture)
        {
            _panFrom = e.GetPosition(MapViewport);
            MapViewport.CaptureMouse();
            MapViewport.Cursor = Cursors.SizeAll;
            e.Handled = true;
        }
    }

    private void OnViewportMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(MapViewport);
        if (_panFrom is { } from)
        {
            _viewport.PanBy(position - from);
            _panFrom = position;
            ApplyViewport();
        }

        _viewModel.ShowCursorAt(_viewport.ScreenToCell(position, MapDrawing.CellPixels));
    }

    private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_panFrom is not null)
        {
            MapViewport.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    // Also reached without a mouse-up, when another window takes the mouse mid-drag, so a drag never outlives its button.
    private void OnViewportLostMouseCapture(object sender, MouseEventArgs e)
    {
        _panFrom = null;
        MapViewport.Cursor = null;
    }

    private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var zoom = e.Delta > 0 ? _viewport.Zoom * WheelZoomStep : _viewport.Zoom / WheelZoomStep;
        _viewport.ZoomAt(e.GetPosition(MapViewport), zoom);
        ApplyViewport();
        e.Handled = true;
    }

    private void OnViewportMouseLeave(object sender, MouseEventArgs e) => _viewModel.ShowCursorAt(null);

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

        // The grid's lines are one screen pixel wide, so they're redrawn when the zoom changes, not on every pan.
        if (_gridZoom != zoom && _viewModel.CurrentMap is { } map)
        {
            GridImage.Source = MapDrawing.DrawGrid(map.Map.Layout, 1 / zoom);
            _gridZoom = zoom;
        }
    }
}
