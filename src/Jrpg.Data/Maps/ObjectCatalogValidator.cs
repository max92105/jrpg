using System.Globalization;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

internal static class ObjectCatalogValidator
{
    private const int MinPolygonPoints = 3;

    public static void Validate(string fileName, IReadOnlyList<ObjectDefinition> objects, IReadOnlySet<int> tileIds) =>
        new ContentListValidator<ObjectDefinition>(fileName, "object", definition => definition.Id)
            .Validate(objects, definition => FindProblem(definition, tileIds));

    private static string? FindProblem(ObjectDefinition definition, IReadOnlySet<int> tileIds)
    {
        if (definition.Width < 1 || definition.Height < 1)
        {
            return $"width and height must be at least 1 (was {definition.Width}x{definition.Height}).";
        }

        if (string.IsNullOrWhiteSpace(definition.Category))
        {
            return "category must not be empty.";
        }

        var pointCount = definition.CollisionPolygon.Count;
        if (pointCount is > 0 and < MinPolygonPoints)
        {
            return $"collisionPolygon must be empty or have at least {MinPolygonPoints} points (had {pointCount}).";
        }

        return FindOutlineProblem("collisionPolygon", definition.CollisionPolygon)
            ?? FindTileProblem(definition, tileIds)
            ?? FindZoneProblem(definition.AbovePlayerZones);
    }

    private static string? FindTileProblem(ObjectDefinition definition, IReadOnlySet<int> tileIds)
    {
        if (definition.Tiles.Count == 0)
        {
            return "tiles must not be empty; an object without tiles draws nothing.";
        }

        var usedCells = new HashSet<(int X, int Y)>();
        foreach (var tile in definition.Tiles)
        {
            if (!tileIds.Contains(tile.TileId))
            {
                return $"tile ({tile.X}, {tile.Y}) has tile id {tile.TileId}, which isn't a tile.";
            }

            if (tile.X < 0 || tile.Y < 0 || tile.X >= definition.Width || tile.Y >= definition.Height)
            {
                return $"tile ({tile.X}, {tile.Y}) must be inside the object's {definition.Width}x{definition.Height} cells.";
            }

            if (!usedCells.Add((tile.X, tile.Y)))
            {
                return $"tile ({tile.X}, {tile.Y}) is listed more than once; an object has one tile per cell.";
            }
        }

        return null;
    }

    private static string? FindZoneProblem(IReadOnlyList<IReadOnlyList<Point2D>> zones)
    {
        for (var index = 0; index < zones.Count; index++)
        {
            // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
            var zone = zones[index];
            if (zone is null || zone.Count < MinPolygonPoints)
            {
                return $"abovePlayerZones[{index}] must have at least {MinPolygonPoints} points.";
            }

            var problem = FindOutlineProblem($"abovePlayerZones[{index}]", zone);
            if (problem is not null)
            {
                return problem;
            }
        }

        return null;
    }

    private static string? FindOutlineProblem(string field, IReadOnlyList<Point2D> outline)
    {
        foreach (var point in outline.Where(point => !IsWithinObject(point)))
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{field} point ({point.X}, {point.Y}) must be within 0..1 on both axes, since points are fractions of the object's size.");
        }

        return null;
    }

    private static bool IsWithinObject(Point2D point) => point.X is >= 0 and <= 1 && point.Y is >= 0 and <= 1;
}
