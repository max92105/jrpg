using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps;
using WpfPolygon = System.Windows.Shapes.Polygon;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The object shapes window: the object's picture with its collision outline and above-player zones over it. Clicks and
/// keys go to the <see cref="ObjectShapesViewModel"/>, which holds the rules; OK closes it only when the shapes can be applied.
/// </summary>
internal sealed partial class ObjectShapesWindow : Window
{
    // Design tunables (legacy ObjectEditorWindow): the object's longer side opens at 560 screen px, between 1x and 6x its
    // map pixels; the wheel zooms 1.2x a notch between 0.5x and 12x. Handles are 10 screen px; a click within 8 of an
    // edge inserts a point; shapes not being edited are drawn at 35 %.
    private const double OpeningScreenPixels = 560;
    private const double MinOpeningZoom = 1;
    private const double MaxOpeningZoom = 6;
    private const double WheelZoomStep = 1.2;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 12;
    private const double HandleScreenPixels = 10;
    private const double InsertScreenPixels = 8;
    private const double InactiveShapeOpacity = 0.35;

    // A mouse wheel notch, in WPF's wheel units.
    private const double WheelNotch = 120;

    private readonly ObjectShapesViewModel _shapes;

    // Screen pixels per map pixel.
    private double _zoom;

    // The point being dragged, by its index in the active shape; null when none is.
    private int? _draggedHandle;

    // Where a Shift + drag rectangle started, in 0..1 over the object; null when none is being drawn.
    private Point2D? _rectangleStart;

    /// <param name="shapes">The shapes being edited.</param>
    /// <param name="picture">The object as the map draws it (<see cref="MapDrawing.DrawObject(Core.Maps.ObjectDefinition)"/>).</param>
    public ObjectShapesWindow(ObjectShapesViewModel shapes, ImageSource picture)
    {
        InitializeComponent();
        DataContext = _shapes = shapes;
        Picture.Source = picture;
        ObjectArea.Width = shapes.Width * MapDrawing.CellPixels;
        ObjectArea.Height = shapes.Height * MapDrawing.CellPixels;
        _zoom = Math.Clamp(OpeningScreenPixels / Math.Max(ObjectArea.Width, ObjectArea.Height), MinOpeningZoom, MaxOpeningZoom);
        shapes.PropertyChanged += OnShapesChanged;
        ApplyZoom();
    }

    // One screen pixel in map pixels, so handles and strokes keep their screen size at any zoom.
    private double ScreenPixel => 1 / _zoom;

    private bool IsDragging => _draggedHandle is not null || _rectangleStart is not null;

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        // The dragged handle's index must stay a point of the shape until the drag ends.
        if (!IsDragging && (e.Key == Key.Back || (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)))
        {
            _shapes.RemoveLastPoint();
            e.Handled = true;
        }
    }

    private void OnShapesChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ObjectShapesViewModel.ZoneEntries) or nameof(ObjectShapesViewModel.SelectedZoneIndex))
        {
            ZoneList.SelectedIndex = _shapes.SelectedZoneIndex;
        }

        if (e.PropertyName == nameof(ObjectShapesViewModel.ActivePoints))
        {
            DrawShapes();
        }
    }

    // The list loses its choice whenever its texts are replaced, so only a choice of a zone is passed on; a list left with
    // none (Ctrl + click) shows the zone the tools still act on again.
    private void OnZoneListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ZoneList.SelectedIndex >= 0)
        {
            _shapes.SelectedZoneIndex = ZoneList.SelectedIndex;
        }
        else if (_shapes.SelectedZoneIndex >= 0 && _shapes.SelectedZoneIndex < ZoneList.Items.Count)
        {
            ZoneList.SelectedIndex = _shapes.SelectedZoneIndex;
        }
    }

    // Legacy controls: Shift + drag draws a rectangle; a handle is dragged, or removed by a right click; a click near an
    // edge inserts a point there; any other click adds one at the end.
    private void OnObjectMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsDragging)
        {
            e.Handled = true;
            return;
        }

        var mouse = OnScreen(e.GetPosition(ObjectArea));
        var handles = _shapes.ActivePoints.Select(point => OnScreen(InPixels(point))).ToList();
        var handle = ShapeEditing.HandleAt(handles, mouse, HandleScreenPixels / 2);
        if (e.ChangedButton == MouseButton.Right)
        {
            if (handle is { } removed)
            {
                _shapes.RemovePoint(removed);
            }
        }
        else if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Shift)
        {
            _rectangleStart = AsFraction(e.GetPosition(ObjectArea));
            ObjectArea.CaptureMouse();
        }
        else if (handle is { } dragged)
        {
            _draggedHandle = dragged;
            ObjectArea.CaptureMouse();
        }
        else if (ShapeEditing.EdgeAt(handles, mouse, InsertScreenPixels) is { } edge)
        {
            _shapes.InsertPoint(edge, AsFraction(e.GetPosition(ObjectArea)));
        }
        else
        {
            _shapes.AddPoint(AsFraction(e.GetPosition(ObjectArea)));
        }

        e.Handled = true;
    }

    private void OnObjectMouseMove(object sender, MouseEventArgs e)
    {
        if (_draggedHandle is { } dragged)
        {
            _shapes.MovePoint(dragged, AsFraction(e.GetPosition(ObjectArea)));
        }
        else if (_rectangleStart is { } start)
        {
            _shapes.SetRectangle(start, AsFraction(e.GetPosition(ObjectArea)));
        }
    }

    private void OnObjectMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (IsDragging && e.ChangedButton == MouseButton.Left)
        {
            ObjectArea.ReleaseMouseCapture();
        }
    }

    // The one place a drag ends: on release, and when the window loses the mouse mid-drag (Alt+Tab).
    private void OnObjectLostMouseCapture(object sender, MouseEventArgs e)
    {
        _draggedHandle = null;
        _rectangleStart = null;
    }

    private void OnViewportMouseWheel(object sender, MouseWheelEventArgs e)
    {
        _zoom = Math.Clamp(_zoom * Math.Pow(WheelZoomStep, e.Delta / WheelNotch), MinZoom, MaxZoom);
        ApplyZoom();
        e.Handled = true;
    }

    // The picture is scaled as a whole, point-sampled; handles and strokes are redrawn to keep their screen size.
    private void ApplyZoom()
    {
        ObjectArea.LayoutTransform = new ScaleTransform(_zoom, _zoom);
        DrawShapes();
    }

    // A position over the object is in map pixels; the view model wants 0..1 over its size.
    private Point2D AsFraction(Point position) => new(position.X / ObjectArea.Width, position.Y / ObjectArea.Height);

    // ShapeEditing measures in screen pixels, so its tolerances don't change with the object's size.
    private Point2D OnScreen(Point position) => new(position.X / ScreenPixel, position.Y / ScreenPixel);

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (_shapes.Problem is null)
        {
            DialogResult = true;
        }
    }

    // In the overlay's colors (E8): every zone blue, the outline green, faint unless being edited, and a round handle on
    // each point of the active shape, the last one yellow so the next click's edge is clear.
    private void DrawShapes()
    {
        OutlineCanvas.Children.Clear();
        for (var index = 0; index < _shapes.Zones.Count; index++)
        {
            var isActive = _shapes.IsEditingZones && index == _shapes.SelectedZoneIndex;
            DrawPolygon(_shapes.Zones[index], CollisionDrawing.ZoneFill, CollisionDrawing.ZoneStroke, isActive);
        }

        DrawPolygon(_shapes.CollisionOutline, CollisionDrawing.OutlineFill, CollisionDrawing.OutlineStroke, _shapes.IsEditingCollision);
        var handleBrush = _shapes.IsEditingZones ? CollisionDrawing.ZoneStroke : CollisionDrawing.OutlineStroke;
        var size = HandleScreenPixels * ScreenPixel;
        var points = _shapes.ActivePoints.Select(InPixels).ToList();
        for (var index = 0; index < points.Count; index++)
        {
            var handle = new Ellipse { Width = size, Height = size, Fill = index == points.Count - 1 ? CollisionDrawing.LastPointHandle : handleBrush };
            Canvas.SetLeft(handle, points[index].X - (size / 2));
            Canvas.SetTop(handle, points[index].Y - (size / 2));
            OutlineCanvas.Children.Add(handle);
        }
    }

    private void DrawPolygon(IReadOnlyList<Point2D> shape, Brush fill, Brush stroke, bool isActive) =>
        OutlineCanvas.Children.Add(new WpfPolygon
        {
            Points = [.. shape.Select(InPixels)],
            Fill = fill,
            Stroke = stroke,
            StrokeThickness = CollisionDrawing.StrokeScreenPixels * ScreenPixel,
            Opacity = isActive ? 1 : InactiveShapeOpacity,
        });

    private Point InPixels(Point2D point) => new(point.X * ObjectArea.Width, point.Y * ObjectArea.Height);
}
