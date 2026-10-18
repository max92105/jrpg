using System.Text.Json;

namespace Jrpg.ContentConverter.Maps;

/// <summary>One tile of a legacy map. Id 0 holds no tile, only a trigger.</summary>
internal sealed record LegacyMapTileJson
{
    public required int Id { get; init; }

    public required int X { get; init; }

    public required int Y { get; init; }

    /// <summary>The draw layer; tiles sharing a z form one layer.</summary>
    public required int Z { get; init; }

    // TODO(plan 2026-10-17-world-exploration-core, slice 3c): convert warp and dialogue triggers into cell events.
    public JsonElement? Trigger { get; init; }
}
