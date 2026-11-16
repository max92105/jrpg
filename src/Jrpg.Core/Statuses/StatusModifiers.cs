using Jrpg.Core.Stats;

namespace Jrpg.Core.Statuses;

/// <summary>
/// What a combatant's statuses do to its stats and to the damage it takes (ADR 0021, ADR 0055): each resistance change
/// raises every resistance by its amount in points (decision 331), each change in damage dealt, such as Weakened's,
/// adds its amount to the damage modifiers' sum (decision 786), each stat change, such as Slowed's, adds its amount in
/// percentage points to its stat (decision 807), and each change in damage taken adds its amount to the share of each
/// hit that lands.
/// </summary>
public static class StatusModifiers
{
    private static readonly int _dodgeCapPermille = StatFormula.PermilleOf(StatFormula.DodgeCapPercent);

    /// <summary>
    /// The stats a stat change may name (decision 807), which <c>statuses.json</c>'s validator reads.
    /// </summary>
    public static IReadOnlyList<StatKind> ChangeableStats { get; } =
        [StatKind.Accuracy, StatKind.Dodge, StatKind.CritChance, StatKind.CrowdControlResistance];

    /// <param name="stats">The combatant's stats for the battle, before statuses.</param>
    /// <param name="statuses">What it holds now.</param>
    public static CombatStats Apply(CombatStats stats, StatusSet statuses) => ChangeableStats.Aggregate(
        stats with
        {
            Resistances = stats.Resistances.Raised(SumOf(statuses, StatusPartKind.ResistanceChange)),

            // Added to the base, so a gear modifier (row 9) keeps its share beside the statuses'.
            DamageModifierPermille =
                stats.DamageModifierPermille + StatFormula.PermilleOf(SumOf(statuses, StatusPartKind.DamageDealtChange)),
        },
        (changed, stat) => WithStatChange(changed, stat, SumOf(statuses, StatusPartKind.StatChange, stat)));

    /// <returns>The extra share of each hit the holder takes, in percent: its damage taken changes, else 0.</returns>
    public static int ExtraDamageTakenPercent(StatusSet statuses) => SumOf(statuses, StatusPartKind.DamageTakenChange);

    // The changes of one stat add up before the bounds hold them, so a rise and a drop cancel out whatever their order.
    private static CombatStats WithStatChange(CombatStats stats, StatKind stat, int points)
    {
        var permille = StatFormula.PermilleOf(points);
        return stat switch
        {
            StatKind.Accuracy => stats with { AccuracyPermille = Math.Max(0, stats.AccuracyPermille + permille) },
            StatKind.Dodge => stats with { DodgePermille = Math.Clamp(stats.DodgePermille + permille, 0, _dodgeCapPermille) },
            StatKind.CritChance => stats with { CritChancePermille = Math.Max(0, stats.CritChancePermille + permille) },
            StatKind.CrowdControlResistance => stats with
            {
                CrowdControlResistancePermille = Math.Max(0, stats.CrowdControlResistancePermille + permille),
            },
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, "A status changes only its changeable stats."),
        };
    }

    // A stat names the one stat change summed; every other kind has none.
    private static int SumOf(StatusSet statuses, StatusPartKind kind, StatKind? stat = null) => statuses.Active
        .SelectMany(status => status.Status.Parts)
        .Where(part => part.Kind == kind && part.Stat == stat)
        .Sum(part => part.Amount);
}
