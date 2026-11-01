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
/// one ally goes to the lowest HP share among those its condition found, without a draw (decision 401); any other
/// single target is one draw.
/// </summary>
internal sealed class EnemyTargeting(IRandom random)
{
    private readonly ChanceRolls _rolls = new(random);

    public Combatant ForAttack(BattleState battle) => ByTauntRule(battle.CurrentActor, battle.LivingOn(BattleSide.Party));

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
    private static IEnumerable<Combatant> HealCandidates(IReadOnlyList<Combatant> legal, TacticCondition? condition, BattleState battle) =>
        condition is null or { Kind: TacticConditionKind.AllyFallen }
            ? legal
            : legal.Intersect(TacticConditions.CombatantsMeeting(condition, battle.CurrentActor, battle));

    // Cross-multiplied, so nothing rounds; a tie keeps the first in battle order.
    private static Combatant LowestShareOf(IEnumerable<Combatant> candidates) =>
        candidates.Aggregate((lowest, next) =>
            next.Vitals.Hp * lowest.Vitals.MaxHp < lowest.Vitals.Hp * next.Vitals.MaxHp ? next : lowest);

    private Combatant ByTauntRule(Combatant enemy, IReadOnlyList<Combatant> candidates)
    {
        var taunting = candidates.Where(member => member.Statuses.Has(StatusKind.Taunt)).ToList();
        var targets = taunting.Count > 0 && !ResistsTaunt(enemy) ? taunting : candidates;
        return targets[random.NextBelow(targets.Count)];
    }

    // A taunt is self-granted, so the enemy rolls at its choice, not at the grant (I2).
    private bool ResistsTaunt(Combatant enemy) =>
        _rolls.Succeeds(enemy.CurrentStats.CrowdControlResistancePermille);
}
