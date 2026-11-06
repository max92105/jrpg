using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Events;

/// <summary>
/// Why a warp's arrival cell may be wrong (decision 507): advice, never a refusal, since the game moves a player who
/// can't stand there to the nearest cell they can.
/// </summary>
internal static class WarpArrivalWarning
{
    /// <returns>
    /// The first that applies: no tile or a blocked tile, inside an object's collision outline, a walk-into event
    /// there; null when the cell is a plain place to stand.
    /// </returns>
    public static string? Of(MapLayout layout, ContentCatalog catalog, GridCell cell)
    {
        var center = new Point2D(cell.X + 0.5, cell.Y + 0.5);
        if (!WalkabilityGrid.Build(layout, catalog.Tiles).IsWalkable(cell))
        {
            return $"The player can't stand on ({cell.X}, {cell.Y}): no tile, or a blocked tile.";
        }

        if (layout.Objects.FirstOrDefault(placed => Polygon.Contains(CollisionShapes.OutlineOf(placed, catalog.Objects[placed.ObjectId]), center)) is { } blocking)
        {
            return $"({cell.X}, {cell.Y}) is inside the collision outline of {blocking.Uid}.";
        }

        return WalkIntoWarning(layout, catalog, cell, center);
    }

    // The game starts nothing on arrival: a cell's walk-into event waits for the player to step off and back on, an
    // object's for the first step, taken inside it.
    private static string? WalkIntoWarning(MapLayout layout, ContentCatalog catalog, GridCell cell, Point2D center)
    {
        if (layout.CellEvents.FirstOrDefault(cellEvent => cellEvent.Cell == cell && cellEvent.Event.Activation == EventActivation.Touch) is { } onCell)
        {
            return $"({cell.X}, {cell.Y}) holds walk-into event {onCell.Event.Id}: stepping off and back on starts it.";
        }

        var onObject = layout.Objects
            .Where(placed => Covers(placed, catalog.Objects[placed.ObjectId], center))
            .SelectMany(placed => placed.Events.Where(mapEvent => mapEvent.Activation == EventActivation.Touch && mapEvent.CanRunOn(placed.ObjectId)))
            .FirstOrDefault();
        return onObject is null ? null : $"({cell.X}, {cell.Y}) is inside an object with walk-into event {onObject.Id}: the first step starts it.";
    }

    private static bool Covers(PlacedObject placed, ObjectDefinition definition, Point2D point) =>
        point.X >= placed.Position.X && point.X < placed.Position.X + definition.Width
        && point.Y >= placed.Position.Y && point.Y < placed.Position.Y + definition.Height;
}
