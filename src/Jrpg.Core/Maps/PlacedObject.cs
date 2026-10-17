using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>One object placed on a map.</summary>
public sealed record PlacedObject
{
    /// <summary>Unique within its map and stable across edits, because saves remember objects by it.</summary>
    public required string Uid { get; init; }

    public required string ObjectId { get; init; }

    /// <summary>The object's top-left corner.</summary>
    public required Point2D Position { get; init; }
}
