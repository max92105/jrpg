using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>Which cells of a map can be walked on, worked out once per map.</summary>
public sealed class WalkabilityGrid
{
    private readonly bool[,] _isWalkable;

    private WalkabilityGrid(bool[,] isWalkable)
    {
        _isWalkable = isWalkable;
    }

    /// <summary>
    /// A cell is walkable when it holds at least one tile and every tile in it is walkable. An
    /// unknown tile id blocks, so a missing tile can't open a hole in a wall.
    /// </summary>
    public static WalkabilityGrid Build(MapLayout layout, IReadOnlyDictionary<int, TileDefinition> tiles)
    {
        var isWalkable = new bool[layout.Width, layout.Height];
        for (var y = 0; y < layout.Height; y++)
        {
            for (var x = 0; x < layout.Width; x++)
            {
                isWalkable[x, y] = IsCellWalkable(layout, tiles, new GridCell(x, y));
            }
        }

        return new WalkabilityGrid(isWalkable);
    }

    /// <summary>Width in cells.</summary>
    public int Width => _isWalkable.GetLength(0);

    /// <summary>Height in cells.</summary>
    public int Height => _isWalkable.GetLength(1);

    public bool IsWalkable(GridCell cell) =>
        cell.X >= 0 && cell.Y >= 0
        && cell.X < _isWalkable.GetLength(0) && cell.Y < _isWalkable.GetLength(1)
        && _isWalkable[cell.X, cell.Y];

    private static bool IsCellWalkable(MapLayout layout, IReadOnlyDictionary<int, TileDefinition> tiles, GridCell cell)
    {
        var tileIds = layout.Layers
            .Select(layer => layer.TileIdAt(cell))
            .Where(id => id != TileLayer.EmptyTileId);
        return tileIds.Any()
            && tileIds.All(id => tiles.TryGetValue(id, out var tile) && tile.IsWalkable);
    }
}
