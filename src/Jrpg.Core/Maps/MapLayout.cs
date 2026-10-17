using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>A map's geometry: its tiles and placed objects.</summary>
public sealed record MapLayout
{
    public required string Id { get; init; }

    /// <summary>Width in cells.</summary>
    public required int Width { get; init; }

    /// <summary>Height in cells.</summary>
    public required int Height { get; init; }

    /// <summary>Bottom layer first.</summary>
    public required IReadOnlyList<TileLayer> Layers { get; init; }

    public required IReadOnlyList<PlacedObject> Objects { get; init; }

    public bool Contains(GridCell cell) => cell.X >= 0 && cell.Y >= 0 && cell.X < Width && cell.Y < Height;
}
