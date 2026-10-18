using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>Where the player lands on entering a map, so a bad warp target can't trap them in a wall (legacy FindArrivalPoint).</summary>
internal static class WarpArrival
{
    /// <summary>How many cells around a blocked target are searched for a walkable one.</summary>
    public const int SearchRadius = 8;

    /// <summary>
    /// The target itself when it's on the map and its cell is walkable. Otherwise the center of the nearest walkable
    /// cell, searched ring by ring from the target's cell clamped to the map (ring 0 is that cell, so an off-map target
    /// next to a walkable edge lands on the edge; legacy skipped it). Within a ring the smallest squared distance wins and
    /// the first found wins a tie (legacy scan order: rows top to bottom, then columns left to right). If no cell within
    /// <see cref="SearchRadius"/> is walkable, the center of the clamped cell.
    /// </summary>
    public static Point2D Find(WalkabilityGrid grid, Point2D target)
    {
        var targetCell = GridCell.Containing(target);
        if (grid.IsWalkable(targetCell))
        {
            return target;
        }

        var clamped = new GridCell(Math.Clamp(targetCell.X, 0, grid.Width - 1), Math.Clamp(targetCell.Y, 0, grid.Height - 1));
        for (var radius = 0; radius <= SearchRadius; radius++)
        {
            var nearest = NearestWalkableInRing(grid, clamped, radius);
            if (nearest is { } cell)
            {
                return cell.Center;
            }
        }

        return clamped.Center;
    }

    private static GridCell? NearestWalkableInRing(WalkabilityGrid grid, GridCell origin, int radius)
    {
        GridCell? nearest = null;
        var nearestDistance = int.MaxValue;
        for (var dy = -radius; dy <= radius; dy++)
        {
            for (var dx = -radius; dx <= radius; dx++)
            {
                var distance = (dx * dx) + (dy * dy);
                var isOnRing = Math.Max(Math.Abs(dx), Math.Abs(dy)) == radius;
                var cell = new GridCell(origin.X + dx, origin.Y + dy);
                if (isOnRing && distance < nearestDistance && grid.IsWalkable(cell))
                {
                    nearest = cell;
                    nearestDistance = distance;
                }
            }
        }

        return nearest;
    }
}
