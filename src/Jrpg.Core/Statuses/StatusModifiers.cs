using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>What a combatant's statuses do to its stats (ADR 0021): Guarded adds its power to defense.</summary>
public static class StatusModifiers
{
    /// <param name="stats">The combatant's stats for the battle, before statuses.</param>
    /// <param name="statuses">What it holds now.</param>
    public static CombatStats Apply(CombatStats stats, StatusSet statuses)
    {
        var defenseBonus = statuses.Active.Where(status => status.Kind == StatusKind.Guarded).Sum(status => status.Power);
        return stats with { Defense = stats.Defense + defenseBonus };
    }
}
