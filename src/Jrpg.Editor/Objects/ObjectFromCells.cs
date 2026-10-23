using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects;

/// <summary>The object a rectangle of sheet cells makes.</summary>
internal static class ObjectFromCells
{
    /// <summary>
    /// One tile per cell, every cell kept, even a fully transparent one (decision 124: the editor doesn't read pixels); no
    /// collision outline and no zones, which the shapes window draws next.
    /// </summary>
    /// <param name="id">A valid, unused object id (<see cref="ObjectIds"/>).</param>
    /// <param name="category">The Objects panel's group.</param>
    /// <param name="cells">The object's size and picture.</param>
    /// <param name="tileIdOf">The tile cut from each cell; every cell is a tile by now.</param>
    public static ObjectDefinition Build(string id, string category, SheetCellRectangle cells, Func<SheetCell, int> tileIdOf) => new()
    {
        Id = id,
        Category = category,
        Width = cells.Columns,
        Height = cells.Rows,
        Tiles = [.. cells.Cells.Select(cell => new ObjectTile(cell.Column - cells.Left, cell.Row - cells.Top, tileIdOf(cell)))],
        CollisionPolygon = [],
        AbovePlayerZones = [],
    };
}
