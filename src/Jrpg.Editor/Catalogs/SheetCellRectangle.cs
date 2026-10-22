namespace Jrpg.Editor.Catalogs;

/// <summary>A rectangle of whole cells of one tilesheet, as chosen with Shift + drag in the Tiles panel.</summary>
/// <param name="SheetId">The tilesheet's id.</param>
/// <param name="Left">The leftmost column.</param>
/// <param name="Top">The top row.</param>
/// <param name="Columns">At least 1.</param>
/// <param name="Rows">At least 1.</param>
internal readonly record struct SheetCellRectangle(string SheetId, int Left, int Top, int Columns, int Rows)
{
    /// <summary>Every cell, row by row from the top left.</summary>
    public IReadOnlyList<SheetCell> Cells
    {
        get
        {
            // A struct's lambdas can't read its members, so they're copied first.
            var (sheetId, left, columns) = (SheetId, Left, Columns);
            return [.. Enumerable.Range(Top, Rows).SelectMany(row => Enumerable.Range(left, columns).Select(column => new SheetCell(sheetId, column, row)))];
        }
    }

    /// <summary>The rectangle with these two cells at opposite corners, in any order.</summary>
    /// <exception cref="ArgumentException">The cells are on different sheets.</exception>
    public static SheetCellRectangle Between(SheetCell corner, SheetCell oppositeCorner)
    {
        if (corner.SheetId != oppositeCorner.SheetId)
        {
            throw new ArgumentException($"The corners are on sheets {corner.SheetId} and {oppositeCorner.SheetId}.", nameof(oppositeCorner));
        }

        var (left, right) = (Math.Min(corner.Column, oppositeCorner.Column), Math.Max(corner.Column, oppositeCorner.Column));
        var (top, bottom) = (Math.Min(corner.Row, oppositeCorner.Row), Math.Max(corner.Row, oppositeCorner.Row));
        return new SheetCellRectangle(corner.SheetId, left, top, right - left + 1, bottom - top + 1);
    }
}
