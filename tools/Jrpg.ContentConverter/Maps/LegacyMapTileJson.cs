using Jrpg.ContentConverter.Events;

namespace Jrpg.ContentConverter.Maps;

/// <summary>One tile of a legacy map. Id 0 holds no tile, only a trigger.</summary>
internal sealed record LegacyMapTileJson
{
    public required int Id { get; init; }

    public required int X { get; init; }

    public required int Y { get; init; }

    /// <summary>The draw layer; tiles sharing a z form one layer.</summary>
    public required int Z { get; init; }

    public LegacyTriggerJson? Trigger { get; init; }
}
