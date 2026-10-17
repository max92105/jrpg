using Jrpg.Core.Geometry;

namespace Jrpg.Core.Maps;

/// <summary>One placeable object type, such as a tree or a chest.</summary>
public sealed record ObjectDefinition
{
    public required string Id { get; init; }

    /// <summary>Width in tiles.</summary>
    public required int Width { get; init; }

    /// <summary>Height in tiles.</summary>
    public required int Height { get; init; }

    /// <summary>
    /// The outline that blocks walking, with 0..1 coordinates over the object's width and height so it
    /// stays right if the object is resized. Empty means the object never blocks.
    /// </summary>
    public required IReadOnlyList<Point2D> CollisionPolygon { get; init; }
}
