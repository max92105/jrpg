using System.Text.Json;

namespace Jrpg.ContentConverter.Maps;

/// <summary>One object placed on a legacy map, with every field the file has.</summary>
internal sealed record LegacyPlacedObjectJson
{
    /// <summary>The legacy int id of the object definition.</summary>
    public required int Id { get; init; }

    public required double X { get; init; }

    public required double Y { get; init; }

    public required int Z { get; init; }

    public IReadOnlyDictionary<string, string>? Meta { get; init; }

    public required IReadOnlyDictionary<string, int> ZOverrides { get; init; }

    /// <summary>Missing on maps saved before uids existed (the Back maps, slice 3c).</summary>
    public string? Uid { get; init; }

    // TODO(plan 2026-10-17-world-exploration-core, slice 3c): convert object events, and the old single event.
    public JsonElement? Events { get; init; }

    public JsonElement? Event { get; init; }

    /// <summary>A computed property the legacy serializer wrote by accident; never read.</summary>
    public int? AbovePlayerZoneZ { get; init; }
}
