using Jrpg.Core.Events;
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

    /// <summary>What the object does when the player interacts with it or touches it; empty for decoration.</summary>
    public IReadOnlyList<MapEvent> Events { get; init; } = [];
}
