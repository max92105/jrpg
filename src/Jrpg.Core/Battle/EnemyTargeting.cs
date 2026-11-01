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
/// results. A skill that takes every target, or aims at the enemy itself, draws nothing and ignores a taunt; any other
/// single target is one draw.
/// </summary>
internal sealed class EnemyTargeting(IRandom random)
{
    private readonly ChanceRolls _rolls = new(random);

    public Combatant ForAttack(BattleState battle) => ByTauntRule(battle.CurrentActor, battle.LivingOn(BattleSide.Party));

    public IReadOnlyList<Combatant> ForSkill(BattleState battle, SkillDefinition skill)
    {
        var choice = battle.TargetChoiceFor(skill);
        if (choice.TakesAll || skill.Effect.Targeting == Targeting.Self)
        {
            return choice.Candidates;
        }

        return skill.Effect.Targeting == Targeting.SingleEnemy
            ? [ByTauntRule(battle.CurrentActor, choice.Candidates)]
            : [choice.Candidates[random.NextBelow(choice.Candidates.Count)]];
    }

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
