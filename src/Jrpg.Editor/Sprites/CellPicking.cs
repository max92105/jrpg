using System.Globalization;
using Jrpg.Core.Sprites;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// Picking a sheet's cells with clicks, shared by the Character sprites page (decision 772) and the strip tool
/// (decision 990): the whole cell under a point of the image, and the line listing the picks.
/// </summary>
internal static class CellPicking
{
    /// <summary>
    /// The whole cell under a point; null outside the whole cells (an image that doesn't split evenly leaves an edge
    /// that isn't one) or while a cell has no size.
    /// </summary>
    /// <param name="point">The point's distance from the image's left and top edges, in image pixels.</param>
    /// <param name="cell">A cell's size in image pixels.</param>
    /// <param name="cut">The columns and rows the image is cut into.</param>
    public static SpriteCell? CellAt((double X, double Y) point, (int Width, int Height) cell, (int Columns, int Rows) cut)
    {
        if (cell.Width < 1 || cell.Height < 1 || point.X < 0 || point.Y < 0)
        {
            return null;
        }

        var column = Math.Floor(point.X / cell.Width);
        var row = Math.Floor(point.Y / cell.Height);
        return column < cut.Columns && row < cut.Rows ? new SpriteCell((int)column, (int)row) : null;
    }

    /// <summary>
    /// "4 cells: (1, 3) (2, 3) (3, 3) (2, 3)": column then row, counted from 1 as the image numbers them.
    /// </summary>
    public static string Line(IReadOnlyList<SpriteCell> cells) =>
        $"{cells.Count} {(cells.Count == 1 ? "cell" : "cells")}"
        + (cells.Count == 0 ? "" : ": " + string.Join(' ', cells.Select(cell => string.Create(
            CultureInfo.InvariantCulture, $"({cell.Column + 1}, {cell.Row + 1})"))));
}
