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

    /// <summary>Its basic attack's raw damage (an enemy has no Power to raise it).</summary>
    public required int Damage { get; init; }

    /// <summary>What the enemy's attack hits as (ADR 0027).</summary>
    public required DamageType DamageType { get; init; }

    /// <summary>Its hit chance against 0 dodge, a whole percent (decision 299): the base is already in it.</summary>
    public required int Accuracy { get; init; }

    /// <summary>
    /// How much a member's hit chance drops, a whole percent, at most <see cref="StatFormula.DodgeCapPercent"/>.
    /// </summary>
    public required int Dodge { get; init; }

    /// <summary>
    /// The chance that its landed hit is a crit, a whole percent; a crit deals the base 150 % (decision 300).
    /// </summary>
    public required int CritChance { get; init; }

    /// <summary>The share of each damage type's hits taken off (ADR 0027); 0 for a type content leaves out.</summary>
    public Resistances Resistances { get; init; } = Resistances.None;

    public required int ExperienceReward { get; init; }
}
