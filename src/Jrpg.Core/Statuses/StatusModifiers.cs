using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>
/// What a combatant's statuses do to its stats and to the damage it takes (ADR 0021, ADR 0055): each resistance change
/// raises every resistance by its amount in points (decision 331), each change in damage dealt, such as Weakened's,
/// adds its amount to the damage modifiers' sum (decision 786), and each change in damage taken adds its amount to the
/// share of each hit that lands.
/// </summary>
public static class StatusModifiers
{
    /// <param name="stats">The combatant's stats for the battle, before statuses.</param>
    /// <param name="statuses">What it holds now.</param>
    public static CombatStats Apply(CombatStats stats, StatusSet statuses) => stats with
    {
        Resistances = stats.Resistances.Raised(SumOf(statuses, StatusPartKind.ResistanceChange)),

        // Added to the base, so a gear modifier (row 9) keeps its share beside the statuses'.
        DamageModifierPermille =
            stats.DamageModifierPermille + StatFormula.PermilleOf(SumOf(statuses, StatusPartKind.DamageDealtChange)),
    };

    /// <returns>The extra share of each hit the holder takes, in percent: its damage taken changes, else 0.</returns>
    public static int ExtraDamageTakenPercent(StatusSet statuses) => SumOf(statuses, StatusPartKind.DamageTakenChange);

    private static int SumOf(StatusSet statuses, StatusPartKind kind) => statuses.Active
        .SelectMany(status => status.Status.Parts)
        .Where(part => part.Kind == kind)
        .Sum(part => part.Amount);
}
