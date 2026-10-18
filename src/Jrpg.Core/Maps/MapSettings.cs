namespace Jrpg.Core.Maps;

/// <summary>A map's settings, kept apart from its layout so they stay small and easy to review (ADR 0011).</summary>
public sealed record MapSettings
{
    public required string Id { get; init; }

    /// <summary>The name shown to the player on entering the map.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Null on maps without random battles, such as towns and houses.</summary>
    public RandomEncounterTable? RandomEncounters { get; init; }
}
