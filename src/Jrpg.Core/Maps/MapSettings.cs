namespace Jrpg.Core.Maps;

/// <summary>A map's settings, kept apart from its layout so they stay small and easy to review (ADR 0011).</summary>
public sealed record MapSettings
{
    public required string Id { get; init; }

    /// <summary>The name shown to the player on entering the map.</summary>
    public required string DisplayName { get; init; }

    /// <summary>The track the map loops, in the music folder; null for silence (decision 419).</summary>
    public string? MusicFile { get; init; }

    /// <summary>
    /// The party level the area is made for, shown beside its name on entering (decision 527); null for none, such as a
    /// town. Display only.
    /// </summary>
    public int? AreaLevel { get; init; }

    /// <summary>
    /// Whether entering the map shows its name and level at the top right (decision 528); true unless the developer
    /// turned it off.
    /// </summary>
    public bool ShowsBanner { get; init; } = true;

    /// <summary>Null on maps without random battles, such as towns and houses.</summary>
    public RandomEncounterTable? RandomEncounters { get; init; }
}
