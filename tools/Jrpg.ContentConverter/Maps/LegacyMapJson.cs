namespace Jrpg.ContentConverter.Maps;

/// <summary>One legacy map file (<c>Assets/Maps/*.json</c>), with every field the file has.</summary>
internal sealed record LegacyMapJson
{
    public required string Name { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    /// <summary>Becomes a per-map encounter table in hand-written settings (slice 4b).</summary>
    public required bool RandomEncounters { get; init; }

    public required IReadOnlyList<LegacyMapTileJson> Tiles { get; init; }

    /// <summary>A computed property the legacy serializer wrote by accident; never read.</summary>
    public required IReadOnlyList<int> UsedTileIds { get; init; }

    public required IReadOnlyList<LegacyPlacedObjectJson> Objects { get; init; }
}
