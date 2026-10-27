namespace Jrpg.Core.Stats;

/// <summary>
/// A combatant's battle stat line. A class, not a struct: the stats later slices add have neutral defaults (accuracy
/// 100 %, crit damage 150 %) that a struct's <c>default</c> would silently zero.
/// </summary>
/// <param name="AttackDamage">The basic attack's raw damage: the weapon's, the unarmed one, or an enemy's.</param>
public sealed record CombatStats(int AttackDamage)
{
    /// <summary>
    /// What the combatant's raw damage and healing are multiplied by, in permille: Power's for a member (decision 296),
    /// 1000 for an enemy.
    /// </summary>
    public int DamagePermille { get; init; } = 1000;

    /// <summary>
    /// The hit chance against 0 dodge, in permille (decision 297): a member's from the sheet, an enemy's from content.
    /// </summary>
    public int AccuracyPermille { get; init; } = 1000;

    /// <summary>How much a foe's hit chance drops, in permille.</summary>
    public int DodgePermille { get; init; }

    /// <summary>What the combatant's basic attack hits as (ADR 0027).</summary>
    public DamageType AttackType { get; init; } = DamageType.Blunt;

    /// <summary>The share of each damage type's hits taken off (ADR 0027).</summary>
    public Resistances Resistances { get; init; } = Resistances.None;
}
