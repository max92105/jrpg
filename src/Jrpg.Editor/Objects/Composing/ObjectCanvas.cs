using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// An object's picture being composed: its size and the sheet cell painted on each of its cells, level by level
/// (decisions 456, 460; ADR 0040).
/// </summary>
internal sealed class ObjectCanvas
{
    /// <summary>The largest size each way; an 80 x 112 Mana Seed tree is 5 x 7 cells.</summary>
    public const int MaxCells = 32;

    private readonly Dictionary<CellOnLevel, SheetCell> _cells;

    private ObjectCanvas(int width, int height, Dictionary<CellOnLevel, SheetCell> cells)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(width, MaxCells);
        ArgumentOutOfRangeException.ThrowIfLessThan(height, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(height, MaxCells);
        (Width, Height, _cells) = (width, height, cells);
    }

    public int Width { get; }

    public int Height { get; }

    /// <summary>The painted cells; a cell on a level missing here is empty on that level.</summary>
    public IReadOnlyDictionary<CellOnLevel, SheetCell> Cells => _cells;

    public bool IsBlank => _cells.Count == 0;

    /// <exception cref="ArgumentOutOfRangeException">A size outside 1 to <see cref="MaxCells"/>.</exception>
    public static ObjectCanvas Empty(int width, int height) => new(width, height, []);

    /// <summary>
    /// The canvas a rectangle of sheet cells makes, its size and picture on the base level (New object from the chosen
    /// cells).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A rectangle over <see cref="MaxCells"/> cells either way.
    /// </exception>
    public static ObjectCanvas Of(SheetCellRectangle cells) =>
        Empty(cells.Columns, cells.Rows).Stamp(0, new GridCell(0, 0), cells);

    /// <summary>An existing object's picture, each tile back to its sheet cell on its level (Edit object).</summary>
    /// <param name="definition">The object; every tile it names is in <paramref name="tiles"/>.</param>
    /// <param name="tiles">The catalog's tiles by id.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// An object over <see cref="MaxCells"/> cells either way.
    /// </exception>
    public static ObjectCanvas Of(ObjectDefinition definition, IReadOnlyDictionary<int, TileDefinition> tiles) =>
        new(definition.Width, definition.Height, definition.Tiles.ToDictionary(
            tile => new CellOnLevel(new GridCell(tile.X, tile.Y), tile.Level),
            tile => SheetCellOf(tiles[tile.TileId])));

    /// <summary>
    /// The rectangle painted on <paramref name="level"/> with its top-left on <paramref name="at"/> (decision 458),
    /// replacing what that level held there and leaving the other levels; its cells past the canvas's edges are left
    /// out.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A level outside <see cref="ObjectTile.MinLevel"/> to <see cref="ObjectTile.MaxLevel"/>.
    /// </exception>
    public ObjectCanvas Stamp(int level, GridCell at, SheetCellRectangle cells)
    {
        ThrowIfOutsideTheLevels(level);
        var painted = new Dictionary<CellOnLevel, SheetCell>(_cells);
        foreach (var cell in cells.Cells)
        {
            var target = new GridCell(at.X + cell.Column - cells.Left, at.Y + cell.Row - cells.Top);
            if (IsInside(target, Width, Height))
            {
                painted[new CellOnLevel(target, level)] = cell;
            }
        }

        return new(Width, Height, painted);
    }

    /// <exception cref="ArgumentOutOfRangeException">
    /// A level outside <see cref="ObjectTile.MinLevel"/> to <see cref="ObjectTile.MaxLevel"/>.
    /// </exception>
    public ObjectCanvas Erase(int level, GridCell at)
    {
        ThrowIfOutsideTheLevels(level);
        var painted = new Dictionary<CellOnLevel, SheetCell>(_cells);
        painted.Remove(new CellOnLevel(at, level));
        return new(Width, Height, painted);
    }

    /// <summary>
    /// The canvas at a new size, every level's cells kept at their place from the top-left and those outside dropped.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A size outside 1 to <see cref="MaxCells"/>.</exception>
    public ObjectCanvas Resize(int width, int height) =>
        new(width, height, _cells.Where(pair => IsInside(pair.Key.Cell, width, height)).ToDictionary());

    /// <summary>
    /// One tile per painted cell, bottom level first as the content lists them (ADR 0040), then row by row from the
    /// top-left as an object built from a rectangle orders them, so a saved object that didn't change compares equal.
    /// </summary>
    /// <param name="tileIdOf">The tile cut from each sheet cell; every painted cell is a tile by now.</param>
    public IReadOnlyList<ObjectTile> TilesOf(Func<SheetCell, int> tileIdOf) =>
        [.. _cells.OrderBy(pair => pair.Key.Level).ThenBy(pair => pair.Key.Cell.Y).ThenBy(pair => pair.Key.Cell.X)
            .Select(pair => new ObjectTile(pair.Key.Cell.X, pair.Key.Cell.Y, tileIdOf(pair.Value), pair.Key.Level))];

    private static void ThrowIfOutsideTheLevels(int level)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(level, ObjectTile.MinLevel);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(level, ObjectTile.MaxLevel);
    }

    private static SheetCell SheetCellOf(TileDefinition tile) => new(tile.SheetId, tile.Column, tile.Row);

    private static bool IsInside(GridCell cell, int width, int height) =>
        cell.X >= 0 && cell.Y >= 0 && cell.X < width && cell.Y < height;
}
