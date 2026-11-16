using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// Whom an enemy aims a chosen action at. An attack, and a skill aimed at one member (decision 400), goes to a random
/// living party member, or a random taunting one while any member taunts (question 209), unless the enemy's crowd
/// control resistance shrugs the taunt off (decision 324). One random call for the target either way, and one more only
/// when a taunt might be resisted, so battles without a taunt, and enemies that resist nothing, keep their seeded
/// results. A skill that takes every target, or aims at the enemy itself, draws nothing and ignores a taunt; a heal on
/// one ally goes to the lowest HP share among those its condition found when it reads its own HP or an ally's, else
/// among all its legal targets, without a draw (decisions 401, 835); any other single target is one draw. A rule with a
/// target picks without a draw (decision 837) among its candidates, which a taunt among the skill's legal foes narrows
/// for a pick among foes (decision 838).
/// </summary>
internal sealed class EnemyTargeting(IRandom random)
{
    private readonly ChanceRolls _rolls = new(random);

    public Combatant ForAttack(BattleState battle) => ByTauntRule(battle.CurrentActor, battle.LivingOn(BattleSide.Party));

    /// <summary>
    /// The pick of a rule with a target, after the taunt's roll when a taunter is among the legal foes.
    /// </summary>
    /// <param name="enemy">The enemy whose turn it is.</param>
    /// <param name="target">The rule's target.</param>
    /// <param name="candidates">
    /// Whom the target may aim at (<see cref="TacticTargetCandidates"/>), at least one.
    /// </param>
    /// <param name="legal">The skill's or the attack's legal targets now, in battle order.</param>
    /// <exception cref="ArgumentOutOfRangeException">A target this switch doesn't know.</exception>
    public Combatant ForRule(
        Combatant enemy, TacticTarget target, IReadOnlyList<Combatant> candidates, IReadOnlyList<Combatant> legal)
    {
        var isAmongFoes = candidates[0].Side != enemy.Side;
        var narrowed = isAmongFoes ? TauntingOrAll(enemy, legal, candidates) : candidates;
        return target switch
        {
            TacticTarget.Self => narrowed.Single(),
            TacticTarget.LowestHpFoe or TacticTarget.LowestHpOnItsSide or TacticTarget.Subject => LowestShareOf(narrowed),
            TacticTarget.HighestHpFoe => HighestShareOf(narrowed),
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unknown tactic target."),
        };
    }

    public IReadOnlyList<Combatant> ForSkill(BattleState battle, SkillDefinition skill, TacticCondition? condition)
    {
        var choice = battle.TargetChoiceFor(skill);
        if (choice.TakesAll || skill.Effect.Targeting == Targeting.Self)
        {
            return choice.Candidates;
        }

        if (skill.Effect is { Kind: EffectKind.RestoreHp, Targeting: Targeting.SingleAlly })
        {
            return [LowestShareOf(HealCandidates(choice.Candidates, condition, battle))];
        }

        return skill.Effect.Targeting == Targeting.SingleEnemy
            ? [ByTauntRule(battle.CurrentActor, choice.Candidates)]
            : [choice.Candidates[random.NextBelow(choice.Candidates.Count)]];
    }

    // The condition and the target agree: a heal picked because a goblin is hurt lands on that goblin (decision 401).
    // Only its own HP or an ally's narrows (decision 835): a trigger naming foes or the fallen would leave nobody, and
    // the lowest share of nobody throws.
    private static IEnumerable<Combatant> HealCandidates(IReadOnlyList<Combatant> legal, TacticCondition? condition, BattleState battle) =>
        condition is { } trigger && NamesWhomAHealReaches(trigger.Kind)
            ? legal.Intersect(TacticConditions.CombatantsMeeting(trigger, battle.CurrentActor, battle))
            : legal;

    private static bool NamesWhomAHealReaches(TacticConditionKind kind) =>
        kind == TacticConditionKind.OwnHpBelow || TacticConditionKinds.ShapeOf(kind).Subject == TacticSubject.Allies;

    // Cross-multiplied in long, so nothing rounds or overflows; a tie keeps the first in battle order.
    private static Combatant LowestShareOf(IEnumerable<Combatant> candidates) =>
        candidates.Aggregate((lowest, next) =>
            (long)next.Vitals.Hp * lowest.Vitals.MaxHp < (long)lowest.Vitals.Hp * next.Vitals.MaxHp ? next : lowest);

    // Cross-multiplied in long, so nothing rounds or overflows; a tie keeps the first in battle order.
    private static Combatant HighestShareOf(IEnumerable<Combatant> candidates) =>
        candidates.Aggregate((highest, next) =>
            (long)next.Vitals.Hp * highest.Vitals.MaxHp > (long)highest.Vitals.Hp * next.Vitals.MaxHp ? next : highest);

    private Combatant ByTauntRule(Combatant enemy, IReadOnlyList<Combatant> candidates)
    {
        var targets = TauntingOrAll(enemy, candidates, candidates);
        return targets[random.NextBelow(targets.Count)];
    }

    // The taunting legal foes replace the candidates unless the enemy shrugs the taunt off, a roll made only when a
    // legal foe taunts.
    private IReadOnlyList<Combatant> TauntingOrAll(
        Combatant enemy, IReadOnlyList<Combatant> legal, IReadOnlyList<Combatant> candidates)
    {
        var taunting = legal
            .Where(member => member.Side != enemy.Side && member.Statuses.FirstWith(StatusPartKind.DrawAttacks) is not null)
            .ToList();
        return taunting.Count > 0 && !ResistsTaunt(enemy) ? taunting : candidates;
    }

    // A taunt is self-granted, so the enemy rolls at its choice, not at the grant (I2).
    private bool ResistsTaunt(Combatant enemy) =>
        _rolls.Succeeds(enemy.CurrentStats.CrowdControlResistancePermille);
}
