using System.Windows;
using System.Windows.Media;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>Draws <see cref="CollisionShapes"/> as one image over the map, in the map image's pixels.</summary>
internal static class CollisionDrawing
{
    // Legacy ObjectEditorWindow's colors: red blocked cells, green collision outlines, blue above-player zones.
    private static readonly Brush _blockedFill = Frozen(new SolidColorBrush(Color.FromArgb(0x60, 0xFF, 0x30, 0x30)));
    private static readonly Brush _outlineStroke = Frozen(new SolidColorBrush(Color.FromRgb(0x7C, 0xFC, 0x7C)));
    private static readonly Brush _outlineFill = Frozen(new SolidColorBrush(Color.FromArgb(0x46, 0x00, 0xFF, 0x00)));
    private static readonly Brush _zoneStroke = Frozen(new SolidColorBrush(Color.FromRgb(0x6E, 0xC8, 0xFF)));
    private static readonly Brush _zoneFill = Frozen(new SolidColorBrush(Color.FromArgb(0x50, 0x00, 0x96, 0xFF)));

    private const double StrokeScreenPixels = 2;

    /// <param name="shapes">What to draw, in map cells.</param>
    /// <param name="layout">The map, for the image's size.</param>
    /// <param name="screenPixel">One screen pixel in map image pixels at the current zoom, so strokes stay 2 pixels wide.</param>
    public static DrawingImage Draw(CollisionShapes shapes, MapLayout layout, double screenPixel)
    {
        var size = new Rect(0, 0, layout.Width * MapDrawing.CellPixels, layout.Height * MapDrawing.CellPixels);
        var drawing = new DrawingGroup();
        using (var context = drawing.Open())
        {
            // Pins the image to the map's top-left, since a drawing's origin follows what it draws.
            context.DrawRectangle(Brushes.Transparent, null, size);
            foreach (var cell in shapes.BlockedCells)
            {
                context.DrawRectangle(_blockedFill, null, CellRect(cell));
            }

            var strokeWidth = StrokeScreenPixels * screenPixel;
            DrawPolygons(context, shapes.Outlines, _outlineFill, new Pen(_outlineStroke, strokeWidth));
            DrawPolygons(context, shapes.Zones, _zoneFill, new Pen(_zoneStroke, strokeWidth));
        }

        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    private static void DrawPolygons(DrawingContext context, IReadOnlyList<IReadOnlyList<Point2D>> polygons, Brush fill, Pen stroke)
    {
        foreach (var polygon in polygons.Where(polygon => polygon.Count >= 3))
        {
            var points = polygon.Select(point => new Point(point.X * MapDrawing.CellPixels, point.Y * MapDrawing.CellPixels)).ToList();
            var figure = new PathFigure(points[0], [new PolyLineSegment(points.Skip(1), isStroked: true)], closed: true);
            context.DrawGeometry(fill, stroke, new PathGeometry([figure]));
        }
    }

    private static Rect CellRect(GridCell cell) =>
        new(cell.X * MapDrawing.CellPixels, cell.Y * MapDrawing.CellPixels, MapDrawing.CellPixels, MapDrawing.CellPixels);

    private static Brush Frozen(Brush brush)
    {
        brush.Freeze();
        return brush;
    }
}
