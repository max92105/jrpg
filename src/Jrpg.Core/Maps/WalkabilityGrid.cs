using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>Which cells of a map can be walked on, worked out once per map.</summary>
internal sealed class WalkabilityGrid
{
    private readonly bool[,] _isWalkable;

    private WalkabilityGrid(bool[,] isWalkable)
    {
        _isWalkable = isWalkable;
    }

    /// <summary>
    /// A cell is walkable when it holds at least one tile and every tile in it is walkable (legacy
    /// <c>MapManager.IsCellWalkable</c>). An unknown tile id blocks, as in legacy.
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
