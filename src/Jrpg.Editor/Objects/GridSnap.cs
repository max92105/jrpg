using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Objects;

/// <summary>
/// Where an object's top-left lands when it snaps to the grid (decision 511): the nearest whole cell, a half going
/// right and down, so an object centered near the map's left edge (a negative top-left) snaps the same way.
/// </summary>
internal static class GridSnap
{
    public static Point2D TopLeft(Point2D topLeft) => new(Math.Floor(topLeft.X + 0.5), Math.Floor(topLeft.Y + 0.5));
}
