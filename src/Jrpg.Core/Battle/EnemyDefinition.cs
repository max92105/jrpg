using Jrpg.Core.Stats;

namespace Jrpg.Core.Battle;

/// <summary>One enemy type. Every enemy of this type starts a battle at full HP.</summary>
public sealed record EnemyDefinition
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string PortraitFile { get; init; }

    public required string BattleSpriteFile { get; init; }

    public required string AttackEffectFile { get; init; }

    public required bool IsSpriteFlipped { get; init; }

    public required int MaxHp { get; init; }

    public required int Attack { get; init; }

    public required int Defense { get; init; }

    /// <summary>What the enemy's attack hits as (ADR 0027).</summary>
    public required DamageType DamageType { get; init; }

    /// <summary>The share of each damage type's hits taken off (ADR 0027); 0 for a type content leaves out.</summary>
    public Resistances Resistances { get; init; } = Resistances.None;

    public required int ExperienceReward { get; init; }
}
