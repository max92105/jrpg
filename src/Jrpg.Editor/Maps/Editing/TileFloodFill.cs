using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// The cells a flood fill reaches: from a start cell, every cell joined to it by edges that holds the same tile on one
/// layer. Empty cells count as a tile of their own, so a fill on an empty area or a layer the map doesn't have yet
/// covers the connected empty cells.
/// </summary>
internal static class TileFloodFill
{
    /// <returns>Each cell once, starting with <paramref name="start"/>; empty when the start is off the map.</returns>
    public static IReadOnlyList<GridCell> Cells(MapLayout map, int layerZ, GridCell start)
    {
        if (!map.Contains(start))
        {
            return [];
        }

        var layer = map.Layers.FirstOrDefault(candidate => candidate.Z == layerZ);
        var startTileId = TileIdAt(layer, start);
        var region = new List<GridCell>();
        var reached = new HashSet<GridCell> { start };
        var waiting = new Queue<GridCell>([start]);
        while (waiting.TryDequeue(out var cell))
        {
            region.Add(cell);
            foreach (var neighbour in NeighboursOf(cell))
            {
                if (map.Contains(neighbour) && TileIdAt(layer, neighbour) == startTileId && reached.Add(neighbour))
                {
                    waiting.Enqueue(neighbour);
                }
            }
        }

        return region;
    }

    private static int TileIdAt(TileLayer? layer, GridCell cell) => layer?.TileIdAt(cell) ?? TileLayer.EmptyTileId;

    private static GridCell[] NeighboursOf(GridCell cell) =>
        [new(cell.X + 1, cell.Y), new(cell.X - 1, cell.Y), new(cell.X, cell.Y + 1), new(cell.X, cell.Y - 1)];
}
