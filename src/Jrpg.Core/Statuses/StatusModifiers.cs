using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>
/// What a combatant's statuses do to its stats and to the damage it takes (ADR 0021): Guarded raises every resistance
/// by its power in points (decision 331), and Exposed adds its power to the share of each hit that lands.
/// </summary>
public static class StatusModifiers
{
    /// <param name="stats">The combatant's stats for the battle, before statuses.</param>
    /// <param name="statuses">What it holds now.</param>
    public static CombatStats Apply(CombatStats stats, StatusSet statuses)
    {
        var guard = statuses.Active.Where(status => status.Kind == StatusKind.Guarded).Sum(status => status.Power);
        return stats with { Resistances = stats.Resistances.Raised(guard) };
    }

    /// <returns>The extra share of each hit the holder takes, in percent: Exposed's power, else 0.</returns>
    public static int ExtraDamageTakenPercent(StatusSet statuses) =>
        statuses.Active.Where(status => status.Kind == StatusKind.Exposed).Sum(status => status.Power);
}
