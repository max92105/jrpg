using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The editor window. Turning the open map into WPF images and mouse and keys into pan and zoom are view concerns, done
/// here; the view models hold plain data and run in tests without a UI thread (ADR 0012). The camera rules are in
/// <see cref="ViewportTransform"/>, the painting rules in <see cref="MapViewModel"/>.
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
    private MouseButton? _strokeButton;
    private GridCell? _brushCell;

    // Where a rectangle drag started; it is filled when its button is released.
    private GridCell? _rectangleStart;
    private double? _gridZoom;
    private MapViewModel? _shownMap;

    // A newly opened map is fitted to the view, which has no size until the window is laid out.
    private bool _isFitPending = true;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        if (viewModel.Catalog is { } catalog)
        {
            var tileImages = new TileImages(catalog, viewModel.ArtRoot);
            _drawing = new MapDrawing(catalog, tileImages);
            TileBrowser.Images = tileImages;
        }

        DataContext = _viewModel = viewModel;
        viewModel.PropertyChanged += OnViewModelChanged;
        OpenCurrentMap();
    }

    /// <summary>The question <see cref="MainViewModel"/> asks before unsaved changes are dropped.</summary>
    public static bool AskToDiscard(string question) =>
        MessageBox.Show(question, "Unsaved changes", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = !_viewModel.ConfirmClose();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        e.Handled = Keyboard.Modifiers == ModifierKeys.Control ? HandleZoomKey(e.Key) : HandleToolKey(e.Key) || HandleViewportKey(e.Key);
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
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.CurrentMap):
                OpenCurrentMap();
                break;
            case nameof(MainViewModel.LayerZ) or nameof(MainViewModel.IsOtherLayersDimmed):
                DrawCurrentMap();
                break;
            case nameof(MainViewModel.BrushSize) or nameof(MainViewModel.Tool):
                ShowBrushAt(_brushCell);
                break;
        }
    }

    private void OnOpenMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.Map))
        {
            DrawCurrentMap();
        }
    }

    private void OpenCurrentMap()
    {
        _shownMap?.PropertyChanged -= OnOpenMapChanged;
        _shownMap = _viewModel.CurrentMap;
        _shownMap?.PropertyChanged += OnOpenMapChanged;

        // Kept in step here rather than bound, so a refused switch simply leaves the list on the open map.
        MapList.SelectedItem = _shownMap?.Map.Id;
        _gridZoom = null;
        DrawCurrentMap();
        RefitMap();
    }

    private void DrawCurrentMap()
    {
        var layout = _shownMap?.Map.Layout;
        var focusZ = _viewModel.IsOtherLayersDimmed ? _viewModel.LayerZ : (int?)null;
        MapImage.Source = layout is not null && _drawing is not null ? _drawing.Draw(layout, focusZ) : null;
    }

    private void OnMapListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (MapList.SelectedItem is string mapId && !_viewModel.TryOpenMap(mapId))
        {
            MapList.SelectedItem = _shownMap?.Map.Id;
        }
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

    // Anywhere but a text box, so tools and brush size change while browsing tiles too.
    private bool HandleToolKey(Key key)
    {
        if (Keyboard.FocusedElement is TextBox)
        {
            return false;
        }

        switch (key)
        {
            case Key.OemOpenBrackets:
                _viewModel.ShrinkBrush();
                return true;
            case Key.OemCloseBrackets:
                _viewModel.GrowBrush();
                return true;
            case Key.B:
                _viewModel.Tool = TileTool.Brush;
                return true;
            case Key.R:
                _viewModel.Tool = TileTool.Rectangle;
                return true;
            case Key.F:
                _viewModel.Tool = TileTool.Fill;
                return true;
            default:
                return false;
        }
    }

    private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
    {
        MapViewport.Focus();
        var isPanGesture = e.ChangedButton == MouseButton.Middle || (e.ChangedButton == MouseButton.Left && _isSpaceHeld);
        if (isPanGesture)
        {
            StartPanning(e.GetPosition(MapViewport));
            e.Handled = true;
        }
        else if (_strokeButton is null)
        {
            e.Handled = HandlePaintButton(e.ChangedButton, CellAt(e));
        }
    }

    private bool HandlePaintButton(MouseButton button, GridCell cell)
    {
        if (button == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.Alt)
        {
            _viewModel.PickTileAt(cell);
            return true;
        }

        // Shift + click fills with any tool, as in the legacy editor.
        if (button == MouseButton.Left && (Keyboard.Modifiers == ModifierKeys.Shift || _viewModel.Tool == TileTool.Fill))
        {
            _viewModel.FloodFill(cell);
            return true;
        }

        return button is MouseButton.Left or MouseButton.Right && StartDrag(button, cell);
    }

    // Left paints, right erases: with the brush as the mouse moves, with the rectangle tool when the button is released.
    private bool StartDrag(MouseButton button, GridCell cell)
    {
        if (_viewModel.Tool == TileTool.Rectangle)
        {
            _rectangleStart = cell;
        }
        else if (button == MouseButton.Left)
        {
            _viewModel.StartPainting(cell);
        }
        else
        {
            _viewModel.StartErasing(cell);
        }

        _strokeButton = button;
        MapViewport.CaptureMouse();
        return true;
    }

    private void FinishRectangle(GridCell start, GridCell end)
    {
        if (_strokeButton == MouseButton.Left)
        {
            _viewModel.PaintRectangle(start, end);
        }
        else
        {
            _viewModel.EraseRectangle(start, end);
        }
    }

    private void StartPanning(Point from)
    {
        _panFrom = from;
        MapViewport.CaptureMouse();
        MapViewport.Cursor = Cursors.SizeAll;
    }

    private void OnViewportMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(MapViewport);
        var cell = CellAt(e);
        if (_panFrom is { } from)
        {
            _viewport.PanBy(position - from);
            _panFrom = position;
            ApplyViewport();
        }
        else if (_strokeButton is not null && _rectangleStart is null)
        {
            _viewModel.ContinueStroke(cell);
        }

        _viewModel.ShowCursorAt(cell);
        ShowBrushAt(cell);
    }

    private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_panFrom is not null || e.ChangedButton == _strokeButton)
        {
            if (_rectangleStart is { } start)
            {
                FinishRectangle(start, CellAt(e));
            }

            MapViewport.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    // Also reached without a mouse-up, when another window takes the mouse mid-drag, so a drag never outlives its button.
    private void OnViewportLostMouseCapture(object sender, MouseEventArgs e)
    {
        _panFrom = null;
        _rectangleStart = null;
        ShowBrushAt(_brushCell);
        MapViewport.Cursor = null;
        if (_strokeButton is not null)
        {
            _strokeButton = null;
            _viewModel.EndStroke();
        }
    }

    private GridCell CellAt(MouseEventArgs e) => _viewport.ScreenToCell(e.GetPosition(MapViewport), MapDrawing.CellPixels);

    /// <summary>Outlines what a click or drag at the cell would change: the brush, the rectangle being dragged, or one cell.</summary>
    /// <param name="cell">The cell under the mouse, or null to hide the outline.</param>
    private void ShowBrushAt(GridCell? cell)
    {
        _brushCell = cell;
        if (cell is not { } at || _viewModel.CurrentMap is null)
        {
            BrushOutline.Visibility = Visibility.Collapsed;
            return;
        }

        var (corner, oppositeCorner) = _rectangleStart is { } start ? (start, at) : SquareCorners(at);
        Canvas.SetLeft(BrushOutline, Math.Min(corner.X, oppositeCorner.X) * MapDrawing.CellPixels);
        Canvas.SetTop(BrushOutline, Math.Min(corner.Y, oppositeCorner.Y) * MapDrawing.CellPixels);
        BrushOutline.Width = (Math.Abs(oppositeCorner.X - corner.X) + 1) * MapDrawing.CellPixels;
        BrushOutline.Height = (Math.Abs(oppositeCorner.Y - corner.Y) + 1) * MapDrawing.CellPixels;
        BrushOutline.StrokeThickness = 2 / _viewport.Zoom;
        BrushOutline.Visibility = Visibility.Visible;
    }

    // Only the brush is wider than one cell.
    private (GridCell Corner, GridCell OppositeCorner) SquareCorners(GridCell center)
    {
        var size = _viewModel.Tool == TileTool.Brush ? _viewModel.BrushSize : 1;
        var topLeft = BrushCells.Square(center, size).First();
        return (topLeft, new GridCell(topLeft.X + size - 1, topLeft.Y + size - 1));
    }

    private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var zoom = e.Delta > 0 ? _viewport.Zoom * WheelZoomStep : _viewport.Zoom / WheelZoomStep;
        _viewport.ZoomAt(e.GetPosition(MapViewport), zoom);
        ApplyViewport();
        e.Handled = true;
    }

    private void OnViewportMouseLeave(object sender, MouseEventArgs e)
    {
        _viewModel.ShowCursorAt(null);
        ShowBrushAt(null);
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

        // The grid's lines are one screen pixel wide, so they're redrawn when the zoom changes, not on every pan.
        if (_gridZoom != zoom && _viewModel.CurrentMap is { } map)
        {
            GridImage.Source = MapDrawing.DrawGrid(map.Map.Layout, 1 / zoom);
            _gridZoom = zoom;
        }
    }
}
