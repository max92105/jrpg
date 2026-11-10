using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>Which placed objects lie under a point or a dragged rectangle, in map cells.</summary>
internal static class PlacedObjectHits
{
    /// <summary>
    /// The objects covering a point, the one drawn on top first (Core's <see cref="MapDrawOrder"/>): one of their tiles
    /// covers it, or their NPC sprite does. An object whose above-player zones hold the point ranks by its canopy,
    /// drawn over everything under the canopy layer (decision 523), so a roof comes before what it hides.
    /// </summary>
    public static IReadOnlyList<PlacedObject> AllAt(MapLayout map, Point2D point, IReadOnlyDictionary<string, ObjectDefinition> definitions)
    {
        var hits = new List<(PlacedObject Placed, DrawOrderKey Key)>();
        for (var index = 0; index < map.Objects.Count; index++)
        {
            var placed = map.Objects[index];
            var definition = definitions[placed.ObjectId];
            if (Covers(placed, definition, point))
            {
                var key = IsInAZone(placed, definition, point)
                    ? MapDrawOrder.CanopyKeyOf(placed, definition, index)
                    : MapDrawOrder.SpriteKeyOf(placed, definition, index);
                hits.Add((placed, key));
            }
        }

        return [.. hits.OrderByDescending(hit => hit.Key).Select(hit => hit.Placed)];
    }

    /// <summary>The object drawn on top at a point; null when no object is there.</summary>
    public static PlacedObject? TopAt(MapLayout map, Point2D point, IReadOnlyDictionary<string, ObjectDefinition> definitions) =>
        AllAt(map, point, definitions) is [var top, ..] ? top : null;

    /// <summary>
    /// The object a Ctrl + click on the same spot again selects (decision 523): the one after the selected object in
    /// the hits, the first again after the last; the first when none is selected or the selected one isn't there.
    /// </summary>
    /// <param name="hits">What <see cref="AllAt"/> found, topmost first.</param>
    /// <param name="selectedUid">The one selected object; null when none or several are.</param>
    public static PlacedObject? NextUnder(IReadOnlyList<PlacedObject> hits, string? selectedUid)
    {
        if (hits.Count == 0)
        {
            return null;
        }

        var selectedIndex = -1;
        for (var index = 0; index < hits.Count; index++)
        {
            if (hits[index].Uid == selectedUid)
            {
                selectedIndex = index;
            }
        }

        return hits[(selectedIndex + 1) % hits.Count];
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

    private static bool IsInAZone(PlacedObject placed, ObjectDefinition definition, Point2D point) =>
        definition.AbovePlayerZones.Any(zone => Polygon.Contains(CollisionShapes.InMapCells(zone, placed, definition), point));

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
