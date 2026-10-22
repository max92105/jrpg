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
    // The picture's box (design tunable), and point marks; both in screen pixels.
    private const double BoxScreenPixels = 560;
    private const double PointMarkScreenPixels = 6;

    private readonly ObjectShapesViewModel _shapes;

    // One screen pixel in map pixels: the object's longer side fills the box.
    private readonly double _screenPixel;

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
        if (e.Key == Key.Back || (e.Key == Key.Z && Keyboard.Modifiers == ModifierKeys.Control))
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

    // A click's position is in map pixels over the object; the view model wants 0..1 over its size.
    private void OnObjectClicked(object sender, MouseButtonEventArgs e)
    {
        var position = e.GetPosition(ObjectArea);
        _shapes.AddPoint(new Point2D(position.X / ObjectArea.Width, position.Y / ObjectArea.Height));
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        if (_shapes.Problem is null)
        {
            DialogResult = true;
        }
    }

    // In the overlay's colors (E8): every zone blue, the outline green, and a mark on each point of the active shape so
    // 1 or 2 points still show.
    private void DrawShapes()
    {
        OutlineCanvas.Children.Clear();
        foreach (var zone in _shapes.Zones)
        {
            DrawPolygon(zone, CollisionDrawing.ZoneFill, CollisionDrawing.ZoneStroke);
        }

        DrawPolygon(_shapes.CollisionOutline, CollisionDrawing.OutlineFill, CollisionDrawing.OutlineStroke);
        var markBrush = _shapes.IsEditingZones ? CollisionDrawing.ZoneStroke : CollisionDrawing.OutlineStroke;
        var markSize = PointMarkScreenPixels * _screenPixel;
        foreach (var point in _shapes.ActivePoints.Select(InPixels))
        {
            var mark = new Rectangle { Width = markSize, Height = markSize, Fill = markBrush };
            Canvas.SetLeft(mark, point.X - (markSize / 2));
            Canvas.SetTop(mark, point.Y - (markSize / 2));
            OutlineCanvas.Children.Add(mark);
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
