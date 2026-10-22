using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>
/// What blocks the player on a map, and the zones drawn over them, in map cells: the overlay of View > Show collision.
/// Blocked cells come from Core's own rule (<see cref="WalkabilityGrid"/>), so the editor shows what the game does.
/// </summary>
/// <param name="BlockedCells">Cells the player can't enter: no tile, or a blocked tile; row by row.</param>
/// <param name="Outlines">Each placed object's collision outline.</param>
/// <param name="Zones">Each placed object's above-player zones, one entry per zone.</param>
internal sealed record CollisionShapes(
    IReadOnlyList<GridCell> BlockedCells,
    IReadOnlyList<IReadOnlyList<Point2D>> Outlines,
    IReadOnlyList<IReadOnlyList<Point2D>> Zones)
{
    public static CollisionShapes Of(MapLayout layout, ContentCatalog catalog)
    {
        var grid = WalkabilityGrid.Build(layout, catalog.Tiles);
        IReadOnlyList<GridCell> blocked =
        [
            .. from y in Enumerable.Range(0, layout.Height)
               from x in Enumerable.Range(0, layout.Width)
               let cell = new GridCell(x, y)
               where !grid.IsWalkable(cell)
               select cell,
        ];
        var placed = layout.Objects.Select(placedObject => (Placed: placedObject, Definition: catalog.Objects[placedObject.ObjectId])).ToList();
        IReadOnlyList<IReadOnlyList<Point2D>> outlines =
        [
            .. placed.Where(pair => pair.Definition.CollisionPolygon.Count > 0).Select(pair => InMapCells(pair.Definition.CollisionPolygon, pair.Placed, pair.Definition)),
        ];
        IReadOnlyList<IReadOnlyList<Point2D>> zones =
        [
            .. placed.SelectMany(pair => pair.Definition.AbovePlayerZones.Select(zone => InMapCells(zone, pair.Placed, pair.Definition))),
        ];
        return new CollisionShapes(blocked, outlines, zones);
    }

    // Shape points are 0..1 over the object's cells, from its top-left corner (as MapCollision reads them).
    private static IReadOnlyList<Point2D> InMapCells(IReadOnlyList<Point2D> shape, PlacedObject placed, ObjectDefinition definition) =>
        [.. shape.Select(point => new Point2D(placed.Position.X + (point.X * definition.Width), placed.Position.Y + (point.Y * definition.Height)))];
}
