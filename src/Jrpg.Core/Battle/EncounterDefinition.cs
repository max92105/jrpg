namespace Jrpg.Core.Battle;

/// <summary>A group of enemies fought together. An enemy's slot on screen is its position in <see cref="EnemyIds"/>.</summary>
public sealed record EncounterDefinition
{
    public required string Id { get; init; }

    public required IReadOnlyList<string> EnemyIds { get; init; }

    public required string BackgroundFile { get; init; }
}
