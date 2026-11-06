using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Maps;

/// <summary>
/// A rectangle of map cells, chosen with Ctrl + drag in Tiles mode to make an object of (decision 514).
/// </summary>
/// <param name="TopLeft">Its top-left cell.</param>
/// <param name="Columns">Its width in cells, at least 1.</param>
/// <param name="Rows">Its height in cells, at least 1.</param>
internal readonly record struct MapCellRectangle(GridCell TopLeft, int Columns, int Rows)
{
    public GridCell BottomRight => new(TopLeft.X + Columns - 1, TopLeft.Y + Rows - 1);

    /// <summary>Its cells, row by row.</summary>
    public IEnumerable<GridCell> Cells => BrushCells.Rectangle(TopLeft, BottomRight);

    /// <param name="corner">One corner, in any order with the other.</param>
    /// <param name="oppositeCorner">The opposite corner.</param>
    public static MapCellRectangle Between(GridCell corner, GridCell oppositeCorner)
    {
        var left = Math.Min(corner.X, oppositeCorner.X);
        var top = Math.Min(corner.Y, oppositeCorner.Y);
        return new MapCellRectangle(new GridCell(left, top), Math.Abs(oppositeCorner.X - corner.X) + 1, Math.Abs(oppositeCorner.Y - corner.Y) + 1);
    }

    /// <summary>The part inside a map; null when the rectangle lies wholly outside it.</summary>
    public MapCellRectangle? ClippedTo(MapLayout layout)
    {
        var left = Math.Max(TopLeft.X, 0);
        var top = Math.Max(TopLeft.Y, 0);
        var right = Math.Min(BottomRight.X, layout.Width - 1);
        var bottom = Math.Min(BottomRight.Y, layout.Height - 1);
        return left > right || top > bottom ? null : Between(new GridCell(left, top), new GridCell(right, bottom));
    }
}
