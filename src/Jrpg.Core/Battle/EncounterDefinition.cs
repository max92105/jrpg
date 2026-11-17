namespace Jrpg.Core.Battle;

/// <summary>A group of enemies fought together. An enemy's slot on screen is its position in <see cref="EnemyIds"/>.</summary>
public sealed record EncounterDefinition
{
    public required string Id { get; init; }

    public required IReadOnlyList<string> EnemyIds { get; init; }

    /// <summary>
    /// The encounter's own background, in the backgrounds folder, a boss's arena, winning over the map's (decision
    /// 892); null for the map's.
    /// </summary>
    public string? BackgroundFile { get; init; }

    /// <summary>
    /// The encounter's own theme, in the music folder, a boss's; null for the battle theme (decision 420).
    /// </summary>
    public string? MusicFile { get; init; }
}
