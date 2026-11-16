namespace Jrpg.Core.Battle;

/// <summary>
/// Whom a rule's target may aim at among the skill's legal targets now, with no draw (decision 836): a rule whose
/// target finds nobody isn't eligible, and the chosen rule's pick runs over these.
/// </summary>
internal static class TacticTargetCandidates
{
    /// <param name="action">A rule with a target.</param>
    /// <param name="legal">Its skill's legal targets now, or the attack's, in battle order.</param>
    /// <param name="enemy">The enemy whose turn it is.</param>
    /// <param name="battle">The battle, which the rule's condition reads.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The rule has no target, or one this switch doesn't know.
    /// </exception>
    public static IReadOnlyList<Combatant> For(
        EnemyAction action, IReadOnlyList<Combatant> legal, Combatant enemy, BattleState battle) =>
        action.On switch
        {
            TacticTarget.Self => [.. legal.Where(candidate => candidate == enemy)],
            TacticTarget.LowestHpFoe or TacticTarget.HighestHpFoe => [.. legal.Where(candidate => candidate.Side != enemy.Side)],
            TacticTarget.LowestHpOnItsSide => [.. legal.Where(candidate => candidate.Side == enemy.Side && candidate.IsAlive)],
            TacticTarget.Subject => SubjectsAmong(legal, action.When, enemy, battle),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action.On, "Unknown tactic target."),
        };

    // Without a condition, "that one" names nobody, so the rule is never eligible (design.md, section 3.3).
    private static IReadOnlyList<Combatant> SubjectsAmong(
        IReadOnlyList<Combatant> legal, TacticCondition? condition, Combatant enemy, BattleState battle) =>
        condition is null ? [] : [.. legal.Intersect(TacticConditions.CombatantsMeeting(condition, enemy, battle))];
}
