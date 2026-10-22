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

    /// <summary>The group the editor's object browser lists it under, such as "House".</summary>
    public required string Category { get; init; }

    /// <summary>The tiles that draw the object, each on its own cell within <see cref="Width"/> x <see cref="Height"/>.</summary>
    public required IReadOnlyList<ObjectTile> Tiles { get; init; }

    /// <summary>
    /// The field sprite an NPC is drawn from instead of tiles (ADR 0015), a file in the field-sprite art folder laid out
    /// like the player's; null for an object drawn from <see cref="Tiles"/>.
    /// </summary>
    public string? FieldSpriteFile { get; init; }

    /// <summary>
    /// Outlines of the parts drawn over the player, such as a canopy or a roof, in the same 0..1 coordinates as
    /// <see cref="CollisionPolygon"/>. Empty means the whole object is drawn by depth.
    /// </summary>
    public required IReadOnlyList<IReadOnlyList<Point2D>> AbovePlayerZones { get; init; }
}
