using Jrpg.Core.Geometry;

namespace Jrpg.Core.Exploration;

/// <summary>Whether a body fits somewhere on the active map. Called every frame, so it never allocates.</summary>
internal sealed class MapCollision(ActiveMap map)
{
    /// <summary>
    /// A body is a square of half-width <paramref name="radius"/>. It fits when every cell under the square is
    /// walkable and its center is outside every object's collision polygon. Only the center is tested
    /// against polygons (placeholder rule).
    /// </summary>
    public bool CanStandAt(Point2D center, double radius) => AreCellsWalkable(center, radius) && !IsInsideAnObject(center);

    /// <summary>The nearest point where the whole body is on the map.</summary>
    public Point2D KeepOnMap(Point2D center, double radius)
    {
        var layout = map.Definition.Layout;
        return new Point2D(
            Math.Clamp(center.X, radius, layout.Width - radius),
            Math.Clamp(center.Y, radius, layout.Height - radius));
    }

    // The body covers [center - radius, center + radius): touching a cell line isn't overlapping the next cell, so a
    // body can stand flush against a wall or the map's right and bottom edges.
    private bool AreCellsWalkable(Point2D center, double radius)
    {
        var top = (int)Math.Floor(center.Y - radius);
        var left = (int)Math.Floor(center.X - radius);
        var bottom = (int)Math.Ceiling(center.Y + radius) - 1;
        var right = (int)Math.Ceiling(center.X + radius) - 1;
        for (var y = top; y <= bottom; y++)
        {
            for (var x = left; x <= right; x++)
            {
                if (!map.Walkability.IsWalkable(new GridCell(x, y)))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsInsideAnObject(Point2D center)
    {
        var placedObjects = map.Definition.Layout.Objects;
        for (var index = 0; index < placedObjects.Count; index++)
        {
            var placed = placedObjects[index];
            var definition = map.CurrentDefinitionOf(placed);
            if (definition.CollisionPolygon.Count == 0)
            {
                continue;
            }

            // Polygon points are fractions of the object's size, measured from its top-left corner.
            var local = new Point2D(
                (center.X - placed.Position.X) / definition.Width,
                (center.Y - placed.Position.Y) / definition.Height);
            var isWithinBounds = local.X is >= 0 and <= 1 && local.Y is >= 0 and <= 1;
            if (isWithinBounds && Polygon.Contains(definition.CollisionPolygon, local))
            {
                return true;
            }
        }

        return false;
    }
}
