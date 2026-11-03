using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// An object's picture being composed: its size and the sheet cell on each of its cells (decisions 456, 460).
/// </summary>
internal sealed class ObjectCanvas
{
    /// <summary>The largest size each way; an 80 x 112 Mana Seed tree is 5 x 7 cells.</summary>
    public const int MaxCells = 32;

    private readonly Dictionary<GridCell, SheetCell> _cells;

    private ObjectCanvas(int width, int height, Dictionary<GridCell, SheetCell> cells)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, MaxCells);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, MaxCells);
        (Width, Height, _cells) = (width, height, cells);
    }

    public int Width { get; }

    public int Height { get; }

    /// <summary>The painted cells; a cell missing here is empty.</summary>
    public IReadOnlyDictionary<GridCell, SheetCell> Cells => _cells;

    public bool IsBlank => _cells.Count == 0;

    /// <exception cref="ArgumentOutOfRangeException">A size outside 1 to <see cref="MaxCells"/>.</exception>
    public static ObjectCanvas Empty(int width, int height) => new(width, height, []);

    /// <summary>
    /// The canvas a rectangle of sheet cells makes, its size and picture (New object from the chosen cells).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A rectangle over <see cref="MaxCells"/> cells either way.
    /// </exception>
    public static ObjectCanvas Of(SheetCellRectangle cells) =>
        Empty(cells.Columns, cells.Rows).Stamp(new GridCell(0, 0), cells);

    /// <summary>An existing object's picture, each tile back to its sheet cell (Edit object).</summary>
    /// <param name="definition">The object; every tile it names is in <paramref name="tiles"/>.</param>
    /// <param name="tiles">The catalog's tiles by id.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// An object over <see cref="MaxCells"/> cells either way.
    /// </exception>
    public static ObjectCanvas Of(ObjectDefinition definition, IReadOnlyDictionary<int, TileDefinition> tiles) =>
        new(definition.Width, definition.Height, definition.Tiles.ToDictionary(
            tile => new GridCell(tile.X, tile.Y),
            tile => SheetCellOf(tiles[tile.TileId])));

    /// <summary>
    /// The rectangle painted with its top-left on <paramref name="at"/> (decision 458), replacing what was there; its
    /// cells past the canvas's edges are left out.
    /// </summary>
    public ObjectCanvas Stamp(GridCell at, SheetCellRectangle cells)
    {
        var painted = new Dictionary<GridCell, SheetCell>(_cells);
        foreach (var cell in cells.Cells)
        {
            var target = new GridCell(at.X + cell.Column - cells.Left, at.Y + cell.Row - cells.Top);
            if (IsInside(target, Width, Height))
            {
                painted[target] = cell;
            }
        }

        return new(Width, Height, painted);
    }

    public ObjectCanvas Erase(GridCell at)
    {
        var painted = new Dictionary<GridCell, SheetCell>(_cells);
        painted.Remove(at);
        return new(Width, Height, painted);
    }

    /// <summary>
    /// The canvas at a new size, its cells kept at their place from the top-left and those outside dropped.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A size outside 1 to <see cref="MaxCells"/>.</exception>
    public ObjectCanvas Resize(int width, int height) =>
        new(width, height, _cells.Where(pair => IsInside(pair.Key, width, height)).ToDictionary());

    /// <summary>
    /// One tile per painted cell, row by row from the top-left as an object built from a rectangle orders them, so a
    /// saved object that didn't change compares equal.
    /// </summary>
    /// <param name="tileIdOf">The tile cut from each sheet cell; every painted cell is a tile by now.</param>
    public IReadOnlyList<ObjectTile> TilesOf(Func<SheetCell, int> tileIdOf) =>
        [.. _cells.OrderBy(pair => pair.Key.Y).ThenBy(pair => pair.Key.X).Select(pair => new ObjectTile(pair.Key.X, pair.Key.Y, tileIdOf(pair.Value)))];

    private static SheetCell SheetCellOf(TileDefinition tile) => new(tile.SheetId, tile.Column, tile.Row);

    private static bool IsInside(GridCell cell, int width, int height) =>
        cell.X >= 0 && cell.Y >= 0 && cell.X < width && cell.Y < height;
}
