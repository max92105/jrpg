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
    // Design tunables (legacy ObjectEditorWindow), in screen pixels: the picture's box, a handle's size, and how near an
    // edge a click inserts a point.
    private const double BoxScreenPixels = 560;
    private const double HandleScreenPixels = 10;
    private const double InsertScreenPixels = 8;

    private readonly ObjectShapesViewModel _shapes;

    // One screen pixel in map pixels: the object's longer side fills the box.
    private readonly double _screenPixel;

    // The point being dragged, by its index in the active shape; null when none is.
    private int? _draggedHandle;

    /// <param name="shapes">The shapes being edited.</param>
    /// <param name="picture">The object as the map draws it (<see cref="MapDrawing.DrawObject(Core.Maps.ObjectDefinition)"/>).</param>
    public ObjectShapesWindow(ObjectShapesViewModel shapes, ImageSource picture)
    {
        InitializeComponent();
        DataContext = _shapes = shapes;
        Picture.Source = picture;
        ObjectArea.Width = shapes.Width * MapDrawing.CellPixels;
        ObjectArea.Height = shapes.Height * MapDrawing.CellPixels;
        ObjectBox.Width = ObjectBox.Height = BoxScreenPixels;
        _screenPixel = Math.Max(ObjectArea.Width, ObjectArea.Height) / BoxScreenPixels;
        shapes.PropertyChanged += OnShapesChanged;
        DrawShapes();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        // The dragged handle's index must stay a point of the shape until the drag ends.
        if (_draggedHandle is null && (e.Key == Key.Back || (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control)))
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

    // Legacy controls: a handle is dragged, or removed by a right click; a click near an edge inserts a point there;
    // any other click adds one at the end.
    private void OnObjectMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_draggedHandle is not null)
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
    }

    private void OnObjectMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_draggedHandle is not null && e.ChangedButton == MouseButton.Left)
        {
            ObjectArea.ReleaseMouseCapture();
        }
    }

    // The one place a drag ends: on release, and when the window loses the mouse mid-drag (Alt+Tab).
    private void OnObjectLostMouseCapture(object sender, MouseEventArgs e) => _draggedHandle = null;

    // A position over the object is in map pixels; the view model wants 0..1 over its size.
    private Point2D AsFraction(Point position) => new(position.X / ObjectArea.Width, position.Y / ObjectArea.Height);

    // ShapeEditing measures in screen pixels, so its tolerances don't change with the object's size.
    private Point2D OnScreen(Point position) => new(position.X / _screenPixel, position.Y / _screenPixel);

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (_shapes.Problem is null)
        {
            DialogResult = true;
        }
    }

    // In the overlay's colors (E8): every zone blue, the outline green, and a round handle on each point of the active
    // shape, the last one yellow so the next click's edge is clear.
    private void DrawShapes()
    {
        OutlineCanvas.Children.Clear();
        foreach (var zone in _shapes.Zones)
        {
            DrawPolygon(zone, CollisionDrawing.ZoneFill, CollisionDrawing.ZoneStroke);
        }

        DrawPolygon(_shapes.CollisionOutline, CollisionDrawing.OutlineFill, CollisionDrawing.OutlineStroke);
        var handleBrush = _shapes.IsEditingZones ? CollisionDrawing.ZoneStroke : CollisionDrawing.OutlineStroke;
        var size = HandleScreenPixels * _screenPixel;
        var points = _shapes.ActivePoints.Select(InPixels).ToList();
        for (var index = 0; index < points.Count; index++)
        {
            var handle = new Ellipse { Width = size, Height = size, Fill = index == points.Count - 1 ? CollisionDrawing.LastPointHandle : handleBrush };
            Canvas.SetLeft(handle, points[index].X - (size / 2));
            Canvas.SetTop(handle, points[index].Y - (size / 2));
            OutlineCanvas.Children.Add(handle);
        }
    }

    private void DrawPolygon(IReadOnlyList<Point2D> shape, Brush fill, Brush stroke) =>
        OutlineCanvas.Children.Add(new WpfPolygon
        {
            Points = [.. shape.Select(InPixels)],
            Fill = fill,
            Stroke = stroke,
            StrokeThickness = CollisionDrawing.StrokeScreenPixels * _screenPixel,
        });

    private Point InPixels(Point2D point) => new(point.X * ObjectArea.Width, point.Y * ObjectArea.Height);
}
