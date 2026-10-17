namespace Jrpg.Core.Maps;

/// <summary>A map's hand-written settings: what the legacy map editor can't express (ADR 0005).</summary>
public sealed record MapSettings
{
    public required string Id { get; init; }

    /// <summary>The name shown to the player on entering the map.</summary>
    public required string DisplayName { get; init; }
}
