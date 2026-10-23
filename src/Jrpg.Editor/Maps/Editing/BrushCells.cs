using Jrpg.Core.Geometry;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>The cells a brush stroke or a rectangle covers.</summary>
internal static class BrushCells
{
    /// <summary>The square brush around a cell; even sizes reach one cell further right and down. It may reach off the map.</summary>
    public static IEnumerable<GridCell> Square(GridCell center, int size)
    {
        var start = -(size - 1) / 2;
        for (var dy = 0; dy < size; dy++)
        {
            for (var dx = 0; dx < size; dx++)
            {
                yield return new GridCell(center.X + start + dx, center.Y + start + dy);
            }
        }
    }

    /// <summary>Every cell of the rectangle between two opposite corners, in any order of the corners.</summary>
    public static IEnumerable<GridCell> Rectangle(GridCell corner, GridCell oppositeCorner)
    {
        for (var y = Math.Min(corner.Y, oppositeCorner.Y); y <= Math.Max(corner.Y, oppositeCorner.Y); y++)
        {
            for (var x = Math.Min(corner.X, oppositeCorner.X); x <= Math.Max(corner.X, oppositeCorner.X); x++)
            {
                yield return new GridCell(x, y);
            }
        }
    }

    /// <summary>
    /// The cells on the straight line between two cells, both included, each touching the previous one (Bresenham), so
    /// a fast drag leaves no gaps between the cells the mouse was seen on.
    /// </summary>
    public static IEnumerable<GridCell> Line(GridCell from, GridCell to)
    {
        var (x, y) = (from.X, from.Y);
        var (dx, dy) = (Math.Abs(to.X - from.X), -Math.Abs(to.Y - from.Y));
        var (stepX, stepY) = (from.X < to.X ? 1 : -1, from.Y < to.Y ? 1 : -1);
        var error = dx + dy;
        while (true)
        {
            yield return new GridCell(x, y);
            if (x == to.X && y == to.Y)
            {
                yield break;
            }

            var doubled = 2 * error;
            if (doubled >= dy)
            {
                error += dy;
                x += stepX;
            }

            if (doubled <= dx)
            {
                error += dx;
                y += stepY;
            }
        }
    }
}
