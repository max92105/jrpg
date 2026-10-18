using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>One layer of a map's tiles, as rows of tile ids.</summary>
public sealed record TileLayer
{
    /// <summary>The id in a cell that holds no tile on this layer.</summary>
    public const int EmptyTileId = 0;

    /// <summary>The draw layer (<see cref="MapLayers"/>); a map's layers are in ascending Z.</summary>
    public required int Z { get; init; }

    /// <summary>One row per map row, top first; each row has one tile id per column.</summary>
    public required IReadOnlyList<IReadOnlyList<int>> Rows { get; init; }

    public int TileIdAt(GridCell cell) => Rows[cell.Y][cell.X];
}
