using Jrpg.Core.Geometry;

namespace Jrpg.ContentConverter.Objects;

/// <summary>One entry of the legacy <c>Objects/Objects.json</c>, with every field the file has.</summary>
internal sealed record LegacyObjectJson
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required string Category { get; init; }

    /// <summary>Stale in the legacy editor: it never updated these after editing the tile group.</summary>
    public required int Width { get; init; }

    public required int Height { get; init; }

    public required IReadOnlyList<LegacyTileGroupEntryJson> Tiles { get; init; }

    /// <summary>0..1 over the tile group's bounds.</summary>
    public required IReadOnlyList<Point2D> CollisionPolygon { get; init; }

    public required IReadOnlyList<IReadOnlyList<Point2D>> AbovePlayerZones { get; init; }

    public required bool IsNpc { get; init; }

    public required IReadOnlyDictionary<string, string> Meta { get; init; }
}
