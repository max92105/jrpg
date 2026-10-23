using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Where a mouse gesture lands on a shape in the shapes window. Points are screen positions, so the tolerances stay in
/// screen pixels whatever the scale and the object's proportions.
/// </summary>
internal static class ShapeEditing
{
    /// <summary>The nearest point within <paramref name="radius"/> of the mouse, to drag or remove; null when none is.</summary>
    public static int? HandleAt(IReadOnlyList<Point2D> points, Point2D mouse, double radius)
    {
        int? nearest = null;
        var nearestDistance = radius;
        for (var index = 0; index < points.Count; index++)
        {
            var distance = Distance(points[index], mouse);
            if (distance <= nearestDistance)
            {
                (nearest, nearestDistance) = (index, distance);
            }
        }

        return nearest;
    }

    /// <summary>
    /// The edge nearest the mouse within <paramref name="distance"/>, as the index of the point it starts from; the
    /// closing edge, from the last point back to the first, counts. Null when no edge is that close.
    /// </summary>
    public static int? EdgeAt(IReadOnlyList<Point2D> points, Point2D mouse, double distance)
    {
        if (points.Count < 2)
        {
            return null;
        }

        int? nearest = null;
        var nearestDistance = distance;
        for (var index = 0; index < points.Count; index++)
        {
            var edgeDistance = DistanceToSegment(mouse, points[index], points[(index + 1) % points.Count]);
            if (edgeDistance <= nearestDistance)
            {
                (nearest, nearestDistance) = (index, edgeDistance);
            }
        }

        return nearest;
    }

    /// <summary>
    /// A point moved to the nearest quarter of the object's cells (decision 126).
    /// </summary>
    /// <param name="point">In 0..1 over the object.</param>
    /// <param name="width">The object's width in cells.</param>
    /// <param name="height">The object's height in cells.</param>
    public static Point2D SnapToQuarterCell(Point2D point, int width, int height) =>
        new(Math.Round(point.X * width * 4) / (width * 4), Math.Round(point.Y * height * 4) / (height * 4));

    private static double Distance(Point2D a, Point2D b) => Math.Sqrt(((a.X - b.X) * (a.X - b.X)) + ((a.Y - b.Y) * (a.Y - b.Y)));

    // The distance to the nearest point of the segment: the projection clamped onto it.
    private static double DistanceToSegment(Point2D point, Point2D start, Point2D end)
    {
        var (dx, dy) = (end.X - start.X, end.Y - start.Y);
        var lengthSquared = (dx * dx) + (dy * dy);
        var along = lengthSquared == 0 ? 0 : Math.Clamp((((point.X - start.X) * dx) + ((point.Y - start.Y) * dy)) / lengthSquared, 0, 1);
        return Distance(point, new Point2D(start.X + (along * dx), start.Y + (along * dy)));
    }
}
