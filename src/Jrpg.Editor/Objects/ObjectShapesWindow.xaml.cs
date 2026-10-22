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
/// The object shapes window: the object's picture with its collision outline over it. Clicks and keys go to the
/// <see cref="ObjectShapesViewModel"/>, which holds the rules; OK closes it only when the outline can be applied.
/// </summary>
internal sealed partial class ObjectShapesWindow : Window
{
    // The picture's box (design tunable), and point marks; both in screen pixels.
    private const double BoxScreenPixels = 560;
    private const double PointMarkScreenPixels = 6;

    private readonly ObjectShapesViewModel _shapes;

    // One screen pixel in map pixels: the object's longer side fills the box.
    private readonly double _screenPixel;

    /// <param name="shapes">The outline being edited.</param>
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
        DrawOutline();
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
        if (e.PropertyName == nameof(ObjectShapesViewModel.CollisionOutline))
        {
            DrawOutline();
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

    // In the overlay's green (E8), with a mark on each point so 1 or 2 points still show.
    private void DrawOutline()
    {
        OutlineCanvas.Children.Clear();
        var points = _shapes.CollisionOutline.Select(point => new Point(point.X * ObjectArea.Width, point.Y * ObjectArea.Height)).ToList();
        OutlineCanvas.Children.Add(new WpfPolygon
        {
            Points = [.. points],
            Fill = CollisionDrawing.OutlineFill,
            Stroke = CollisionDrawing.OutlineStroke,
            StrokeThickness = CollisionDrawing.StrokeScreenPixels * _screenPixel,
        });
        var markSize = PointMarkScreenPixels * _screenPixel;
        foreach (var point in points)
        {
            var mark = new Rectangle { Width = markSize, Height = markSize, Fill = CollisionDrawing.OutlineStroke };
            Canvas.SetLeft(mark, point.X - (markSize / 2));
            Canvas.SetTop(mark, point.Y - (markSize / 2));
            OutlineCanvas.Children.Add(mark);
        }
    }
}
