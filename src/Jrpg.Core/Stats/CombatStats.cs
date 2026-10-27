namespace Jrpg.Core.Stats;

/// <summary>
/// A combatant's battle stat line. A class, not a struct: the stats later slices add have neutral defaults (accuracy
/// 100 %, crit damage 150 %) that a struct's <c>default</c> would silently zero.
/// </summary>
public sealed record CombatStats(int Attack, int Defense)
{
    /// <summary>What the combatant's basic attack hits as (ADR 0027).</summary>
    public DamageType AttackType { get; init; } = DamageType.Blunt;
}
