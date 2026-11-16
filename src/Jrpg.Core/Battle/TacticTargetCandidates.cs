using Jrpg.Core.Skills;

namespace Jrpg.Core.Battle;

/// <summary>
/// Whom a rule's target may aim at among the skill's legal targets now, with no draw (decision 836): a rule whose
/// target finds nobody isn't eligible, and the chosen rule's pick runs over these.
/// </summary>
internal static class TacticTargetCandidates
{
    /// <param name="action">A rule with a target.</param>
    /// <param name="skill">The skill it names, null for the attack.</param>
    /// <param name="legal">Its skill's legal targets now, or the attack's, in battle order.</param>
    /// <param name="battle">The battle on the enemy's turn, which the rule's condition reads.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The rule has no target, or one this switch doesn't know.
    /// </exception>
    public static IReadOnlyList<Combatant> For(
        EnemyAction action, SkillDefinition? skill, IReadOnlyList<Combatant> legal, BattleState battle)
    {
        var enemy = battle.CurrentActor;
        return action.On switch
        {
            TacticTarget.Self => [.. legal.Where(candidate => candidate == enemy)],
            TacticTarget.LowestHpFoe or TacticTarget.HighestHpFoe => [.. legal.Where(candidate => candidate.Side != enemy.Side)],
            TacticTarget.LowestHpOnItsSide => [.. legal.Where(candidate => candidate.Side == enemy.Side && candidate.IsAlive)],
            TacticTarget.Subject => SubjectsAmong(legal, action.When, enemy, battle),
            TacticTarget.UnafflictedFoe => UnafflictedFoesAmong(legal, skill, enemy),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action.On, "Unknown tactic target."),
        };
    }

    // Without a condition, "that one" names nobody, so the rule is never eligible (design.md, section 3.3).
    private static IReadOnlyList<Combatant> SubjectsAmong(
        IReadOnlyList<Combatant> legal, TacticCondition? condition, Combatant enemy, BattleState battle) =>
        condition is null ? [] : [.. legal.Intersect(TacticConditions.CombatantsMeeting(condition, enemy, battle))];

    // Read by family, as a holder keeps one status per family (decision 800): another tier of the poison it grants
    // counts as held. A boon the skill also grants isn't what the rule spreads, so holding it doesn't count.
    private static IReadOnlyList<Combatant> UnafflictedFoesAmong(
        IReadOnlyList<Combatant> legal, SkillDefinition? skill, Combatant enemy)
    {
        var harmfulFamilies = (skill?.Effect.Statuses ?? [])
            .Where(grant => grant.Status.IsHarmful)
            .Select(grant => grant.Status.FamilyKey)
            .ToList();
        return [.. legal.Where(candidate => candidate.Side != enemy.Side && !harmfulFamilies.Exists(candidate.Statuses.Has))];
    }
}
