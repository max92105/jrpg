using System.Text.Json;
using Jrpg.ContentConverter.Events;

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

    /// <summary>Missing on maps saved before uids existed, which the converter rejects.</summary>
    public string? Uid { get; init; }

    public IReadOnlyList<LegacyMapEventJson> Events { get; init; } = [];

    /// <summary>The single event of maps saved before objects had several; no live map has one, so it's rejected.</summary>
    public JsonElement? Event { get; init; }

    /// <summary>A computed property the legacy serializer wrote by accident; never read.</summary>
    public int? AbovePlayerZoneZ { get; init; }
}
