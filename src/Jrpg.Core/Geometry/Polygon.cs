namespace Jrpg.Core.Geometry;

public static class Polygon
{
    /// <summary>
    /// Whether <paramref name="point"/> lies inside the polygon, by the even-odd rule: a ray cast to the right
    /// crosses the outline an odd number of times. Works for concave outlines. The crossing divides by the edge's
    /// height with no guard: a horizontal edge never passes the straddle test, so the height is never zero there.
    /// </summary>
    public static bool Contains(IReadOnlyList<Point2D> vertices, Point2D point)
    {
        var isInside = false;
        for (int current = 0, previous = vertices.Count - 1; current < vertices.Count; previous = current++)
        {
            var a = vertices[current];
            var b = vertices[previous];
            var straddlesRay = (a.Y > point.Y) != (b.Y > point.Y);
            if (straddlesRay && point.X < ((b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y)) + a.X)
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }
}
