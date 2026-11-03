using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects;

/// <summary>The tiles a rectangle of sheet cells gives an object, for Replace cells.</summary>
internal static class ObjectFromCells
{
    /// <summary>
    /// Each cell's tile at its column and row within the rectangle (decision 449).
    /// </summary>
    /// <param name="cells">The rectangle of sheet cells.</param>
    /// <param name="tileIdOf">The tile cut from each cell; every cell is a tile by now.</param>
    public static IReadOnlyList<ObjectTile> TilesOf(SheetCellRectangle cells, Func<SheetCell, int> tileIdOf) =>
        [.. cells.Cells.Select(cell => new ObjectTile(cell.Column - cells.Left, cell.Row - cells.Top, tileIdOf(cell)))];
}
