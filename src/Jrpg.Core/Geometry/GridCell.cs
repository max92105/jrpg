namespace Jrpg.Core.Geometry;

/// <summary>One map cell, by column and row.</summary>
public readonly record struct GridCell(int X, int Y)
{
    public Point2D Center => new(X + 0.5, Y + 0.5);

    /// <summary>The cell a point lies in; a point on a cell's left or top edge belongs to that cell.</summary>
    public static GridCell Containing(Point2D point) => new((int)Math.Floor(point.X), (int)Math.Floor(point.Y));
}
