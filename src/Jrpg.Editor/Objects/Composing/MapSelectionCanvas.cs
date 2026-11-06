using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Objects.Composing;

/// <summary>
/// The composer canvas a rectangle of map cells makes (decision 514): every layer with a tile in it, on its level
/// around the base layer (the one chosen in the tool bar, level 0): the layers under it on -1, -2 and down from the
/// nearest, those over it on 1, 2 and up; a layer with no tile there takes no level. Each map tile becomes the sheet
/// cell it was cut from.
/// </summary>
internal static class MapSelectionCanvas
{
    /// <summary>
    /// The layers with a tile in the rectangle, and the base layer, bottom first, each with its level; read by the
    /// canvas, the composer's warning and the replacement, so the levels are worked out once.
    /// </summary>
    public static IReadOnlyList<(int Z, int Level)> TakenLayersOf(MapLayout layout, MapCellRectangle cells, int baseZ)
    {
        List<int> taken = [.. layout.Layers.Where(layer => HasTileIn(layer, cells)).Select(layer => layer.Z).Append(baseZ).Distinct().Order()];
        var baseIndex = taken.IndexOf(baseZ);
        return [.. taken.Select((z, index) => (z, index - baseIndex))];
    }

    /// <summary>Why the rectangle can't make an object; null when it can.</summary>
    public static string? ProblemOf(MapLayout layout, MapCellRectangle cells, int baseZ)
    {
        if (!layout.Layers.Any(layer => HasTileIn(layer, cells)))
        {
            return "No tile in the selection.";
        }

        if (cells.Columns > ObjectCanvas.MaxCells || cells.Rows > ObjectCanvas.MaxCells)
        {
            return $"The selection is {cells.Columns} x {cells.Rows} cells; an object is at most {ObjectCanvas.MaxCells} each way.";
        }

        var levels = TakenLayersOf(layout, cells, baseZ).Select(layer => layer.Level).ToList();
        var under = levels.Count(level => level < 0);
        if (under > -ObjectTile.MinLevel)
        {
            return $"The selection has tiles on {under} layers under z {baseZ}; an object has {-ObjectTile.MinLevel} levels under its base.";
        }

        var over = levels.Count(level => level > 0);
        return over > ObjectTile.MaxLevel
            ? $"The selection has tiles on {over} layers over z {baseZ}; an object has {ObjectTile.MaxLevel} levels over its base."
            : null;
    }

    /// <exception cref="ArgumentException">
    /// The rectangle can't make an object; the message is <see cref="ProblemOf"/>'s.
    /// </exception>
    public static ObjectCanvas Of(MapLayout layout, MapCellRectangle cells, int baseZ, IReadOnlyDictionary<int, TileDefinition> tiles)
    {
        if (ProblemOf(layout, cells, baseZ) is { } problem)
        {
            throw new ArgumentException(problem, nameof(cells));
        }

        var levels = TakenLayersOf(layout, cells, baseZ).ToDictionary(layer => layer.Z, layer => layer.Level);
        var canvasCells = new Dictionary<CellOnLevel, SheetCell>();
        foreach (var layer in layout.Layers.Where(layer => levels.ContainsKey(layer.Z)))
        {
            foreach (var cell in cells.Cells.Where(cell => layer.TileIdAt(cell) != TileLayer.EmptyTileId))
            {
                var onCanvas = new GridCell(cell.X - cells.TopLeft.X, cell.Y - cells.TopLeft.Y);
                canvasCells[new CellOnLevel(onCanvas, levels[layer.Z])] = SheetCell.Of(tiles[layer.TileIdAt(cell)]);
            }
        }

        return ObjectCanvas.OfCells(cells.Columns, cells.Rows, canvasCells);
    }

    private static bool HasTileIn(TileLayer layer, MapCellRectangle cells) =>
        cells.Cells.Any(cell => layer.TileIdAt(cell) != TileLayer.EmptyTileId);
}
