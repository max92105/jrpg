using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Settings;
using Jrpg.Editor.Tiles;
using Jrpg.Editor.Viewport;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The editor window. Turning the open map into WPF images and mouse and keys into pan and zoom are view concerns, done
/// in this class's partial files; the view models hold plain data and run in tests without a UI thread (ADR 0012). The
/// camera rules are in <see cref="ViewportTransform"/>, the painting rules in <see cref="MapViewModel"/>.
/// </summary>
internal sealed partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly MapDrawing? _drawing;
    private readonly ViewportTransform _viewport = new();
    private bool _isSpaceHeld;
    private MouseButton? _strokeButton;
    private Point2D? _mapPoint;

    private MapViewModel? _shownMap;

    private ControlsHelpWindow? _controlsHelp;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        if (viewModel.Content is { } content)
        {
            var tileImages = new TileImages(content, viewModel.ArtRoot, () => viewModel.Art!.Missing);
            _drawing = new MapDrawing(content, tileImages);
            TileBrowser.Images = tileImages;
            CompositionTarget.Rendering += OnRendering;
            TileBrowser.Marking = viewModel.Marking;
            ObjectBrowser.Drawing = _drawing;
            ObjectBrowser.AnimationTime = viewModel.AnimationTime;
            content.PropertyChanged += OnContentChanged;
            viewModel.SheetImagesImported += (_, files) => RedrawImportedImages(tileImages, files);
        }

        DataContext = _viewModel = viewModel;
        viewModel.PropertyChanged += OnViewModelChanged;
        viewModel.Layer.PropertyChanged += (_, _) => DrawCurrentMap();
        viewModel.Painting?.PropertyChanged += (_, _) => ShowBrushAt(_brushCell);
        viewModel.Placing?.PropertyChanged += (_, _) => DrawSelectionMarks();
        viewModel.Removal?.PropertyChanged += OnRemovalChanged;
        OpenCurrentMap();
    }

    /// <summary>The yes or no questions <see cref="MainViewModel"/> asks, such as before unsaved changes are dropped.</summary>
    public static bool Ask(string question) =>
        MessageBox.Show(question, "Map editor", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        e.Cancel = !_viewModel.ConfirmClose();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        // Space arms the pan whichever panel has focus, so Space + drag pans after a click in the Tiles or Objects panel;
        // in a text box it's only a typed space.
        _isSpaceHeld |= e.Key == Key.Space && Keyboard.FocusedElement is not TextBox;
        e.Handled = Keyboard.Modifiers == ModifierKeys.Control ? HandleZoomKey(e.Key) : HandleToolKey(e.Key) || HandleObjectKey(e.Key) || HandleViewportKey(e.Key);
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
            case nameof(MainViewModel.Mode):
                ShowBrushAt(_brushCell);
                ShowObjectDragAt(_mapPoint);
                break;
            case nameof(MainViewModel.IsCollisionShown):
                DrawCollision();
                break;
        }
    }

    // A catalog change can change art the map shows without changing the map: a re-cut sheet.
    private void OnContentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CatalogDraft.Current))
        {
            DrawCurrentMap();
            ObjectBrowser.ShowPreview();
        }
    }

    private void OnOpenMapChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MapViewModel.Map))
        {
            DrawCurrentMap();
            DrawSelectionMarks();
            DrawEventMarks();

            // A resize, or its undo: show the whole map at its new size.
            if (_gridDrawnFor is { } grid && _shownMap!.Map.Layout is var layout && (grid.Width, grid.Height) != (layout.Width, layout.Height))
            {
                RefitMap();
            }
        }
    }

    private void DrawCurrentMap()
    {
        var layout = _shownMap?.Map.Layout;
        var focusZ = _viewModel.Layer.AreOtherLayersDimmed ? _viewModel.Layer.Z : (int?)null;
        MapImage.Source = layout is not null && _drawing is not null ? _drawing.Draw(layout, focusZ) : null;
        DrawCollision();
    }

    // Drawn with the map, so every map or catalog change and its undo shows on it; strokes follow the zoom.
    private void DrawCollision() =>
        CollisionImage.Source = _viewModel.IsCollisionShown && _shownMap?.Map.Layout is { } layout && _viewModel.Catalog is { } catalog
            ? CollisionDrawing.Draw(CollisionShapes.Of(layout, catalog), layout, 1 / _viewport.Zoom)
            : null;

    private void OnAddLayerClicked(object sender, RoutedEventArgs e) => _viewModel.Layer.TryAddLayer(NewLayerBox.Text);

    private void OnAddLayerMenuClicked(object sender, RoutedEventArgs e) => NewLayerBox.Focus();

    // Closing goes through OnClosing, so unsaved changes still ask.
    private void OnExitClicked(object sender, RoutedEventArgs e) => Close();

    // One controls window, kept open beside the map: F1 again brings it to the front.
    private void OnControlsHelpExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (_controlsHelp is not { IsVisible: true })
        {
            _controlsHelp = new ControlsHelpWindow { Owner = this };
            _controlsHelp.Show();
        }

        _controlsHelp.Activate();
    }

    private void OnNewMapExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (_viewModel.StartNewMap() is { } newMap)
        {
            new NewMapWindow(newMap, _viewModel.TryCreateMap) { Owner = this }.ShowDialog();
        }
    }

    private void OnMapSettingsClicked(object sender, RoutedEventArgs e)
    {
        if (_viewModel.StartEditingSettings() is { } settings)
        {
            new MapSettingsWindow(settings, _viewModel.TryApplySettings) { Owner = this }.ShowDialog();
        }
    }

    private void OnViewportMouseDown(object sender, MouseButtonEventArgs e)
    {
        HideDestinationMark();
        MapViewport.Focus();
        var isPanGesture = e.ChangedButton == MouseButton.Middle || (e.ChangedButton == MouseButton.Left && _isSpaceHeld);
        if (isPanGesture)
        {
            StartPanning(e.GetPosition(MapViewport));
            e.Handled = true;
        }
        else if (_strokeButton is null && _viewModel.CurrentMap is not null)
        {
            e.Handled = _viewModel.Mode switch
            {
                EditingMode.Objects => HandleObjectButton(e.ChangedButton, MapPointAt(e), e.ClickCount),
                EditingMode.Events => HandleEventButton(e.ChangedButton, MapPointAt(e)),
                _ => HandlePaintButton(e.ChangedButton, CellAt(e)),
            };
        }
    }

    private void OnViewportMouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(MapViewport);
        var cell = CellAt(e);
        _mapPoint = MapPointAt(e);
        if (_panFrom is { } from)
        {
            _viewport.PanBy(position - from);
            _panFrom = position;
            ApplyViewport();
        }
        else if (_strokeButton is not null && _rectangleStart is null)
        {
            Painting.ContinueStroke(cell);
        }

        _viewModel.ShowCursorAt(cell);
        ShowBrushAt(cell);
        ShowObjectDragAt(_mapPoint);
    }

    private void OnViewportMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_panFrom is not null || e.ChangedButton == _strokeButton)
        {
            if (_rectangleStart is { } start)
            {
                FinishRectangle(start, CellAt(e));
            }

            FinishObjectDrag(MapPointAt(e));

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
        CancelObjectDrag();
        MapViewport.Cursor = null;
        if (_strokeButton is not null)
        {
            _strokeButton = null;
            Painting.EndStroke();
        }
    }

    // Painting exists whenever a map is open, which the mouse and key handlers check first.
    private TilePaintingViewModel Painting => _viewModel.Painting!;

    private void OnViewportMouseLeave(object sender, MouseEventArgs e)
    {
        _mapPoint = null;
        _viewModel.ShowCursorAt(null);
        ShowBrushAt(null);
        ShowObjectDragAt(null);
    }
}
