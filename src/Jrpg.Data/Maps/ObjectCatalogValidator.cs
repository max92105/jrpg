using System.Globalization;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Maps;

internal static class ObjectCatalogValidator
{
    private const int MinPolygonPoints = 3;

    public static void Validate(string fileName, IReadOnlyList<ObjectDefinition> objects) =>
        new ContentListValidator<ObjectDefinition>(fileName, "object", definition => definition.Id)
            .Validate(objects, FindProblem);

    private static string? FindProblem(ObjectDefinition definition)
    {
        if (definition.Width < 1 || definition.Height < 1)
        {
            return $"width and height must be at least 1 (was {definition.Width}x{definition.Height}).";
        }

        var pointCount = definition.CollisionPolygon.Count;
        if (pointCount is > 0 and < MinPolygonPoints)
        {
            return $"collisionPolygon must be empty or have at least {MinPolygonPoints} points (had {pointCount}).";
        }

        foreach (var point in definition.CollisionPolygon.Where(point => !IsWithinObject(point)))
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"collisionPolygon point ({point.X}, {point.Y}) must be within 0..1 on both axes, since points are fractions of the object's size.");
        }

        return null;
    }

    private static bool IsWithinObject(Point2D point) => point.X is >= 0 and <= 1 && point.Y is >= 0 and <= 1;
}
