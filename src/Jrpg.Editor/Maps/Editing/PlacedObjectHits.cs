using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>Which placed objects lie under a point or a dragged rectangle, in map cells.</summary>
internal static class PlacedObjectHits
{
    /// <summary>
    /// The object drawn on top at a point: one of its tiles covers it, or its NPC sprite does; among several, the one
    /// drawn last in Core's <see cref="DrawOrderKey"/> order. Null when no object is there.
    /// </summary>
    public static PlacedObject? TopAt(MapLayout map, Point2D point, IReadOnlyDictionary<string, ObjectDefinition> definitions)
    {
        PlacedObject? top = null;
        DrawOrderKey? topKey = null;
        for (var index = 0; index < map.Objects.Count; index++)
        {
            var placed = map.Objects[index];
            var definition = definitions[placed.ObjectId];
            var key = DrawOrderKey.ForSprite(placed.Z, placed.Position.Y + definition.Height, index);
            if (Covers(placed, definition, point) && (topKey is null || key > topKey))
            {
                (top, topKey) = (placed, key);
            }
        }

        return top;
    }

    /// <summary>The objects whose bounds overlap the rectangle between two opposite corners, in placement order.</summary>
    public static IReadOnlyList<PlacedObject> InRectangle(
        MapLayout map, Point2D corner, Point2D oppositeCorner, IReadOnlyDictionary<string, ObjectDefinition> definitions)
    {
        var (left, right) = (Math.Min(corner.X, oppositeCorner.X), Math.Max(corner.X, oppositeCorner.X));
        var (top, bottom) = (Math.Min(corner.Y, oppositeCorner.Y), Math.Max(corner.Y, oppositeCorner.Y));
        return [.. map.Objects.Where(placed =>
        {
            var definition = definitions[placed.ObjectId];
            return placed.Position.X < right && placed.Position.X + definition.Width > left
                && placed.Position.Y < bottom && placed.Position.Y + definition.Height > top;
        })];
    }

    // An NPC has no tiles: its sprite covers all its cells (ADR 0015).
    private static bool Covers(PlacedObject placed, ObjectDefinition definition, Point2D point) =>
        definition.FieldSpriteFile is not null
            ? point.X >= placed.Position.X && point.X < placed.Position.X + definition.Width
                && point.Y >= placed.Position.Y && point.Y < placed.Position.Y + definition.Height
            : CoversWithATile(placed, definition, point);

    private static bool CoversWithATile(PlacedObject placed, ObjectDefinition definition, Point2D point) =>
        definition.Tiles.Any(tile =>
            point.X >= placed.Position.X + tile.X && point.X < placed.Position.X + tile.X + 1
            && point.Y >= placed.Position.Y + tile.Y && point.Y < placed.Position.Y + tile.Y + 1);
}
