using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects;

/// <summary>
/// An object with its cells replaced by a sheet rectangle (decision 449): its id, category, collision outline and
/// above-player zones stay, so every placement and every event naming it is untouched. The outline and zones are in the
/// object's own 0-1 coordinates, so they stretch with a new size until the Shapes window adjusts them.
/// </summary>
internal static class ObjectReskin
{
    /// <param name="original">The object re-skinned.</param>
    /// <param name="cells">The rectangle of sheet cells giving its new picture and size.</param>
    /// <param name="tileIdOf">Each cell's tile; the caller makes a cell a tile first when it isn't one yet.</param>
    public static ObjectDefinition Reskin(ObjectDefinition original, SheetCellRectangle cells, Func<SheetCell, int> tileIdOf) => original with
    {
        Width = cells.Columns,
        Height = cells.Rows,
        Tiles = ObjectFromCells.TilesOf(cells, tileIdOf),
    };
}
