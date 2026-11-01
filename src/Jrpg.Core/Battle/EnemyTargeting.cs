using Jrpg.Core.Randomness;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// Whom an enemy aims a chosen action at. An attack aims at a random living party member, or a random taunting one
/// while any member taunts (question 209), unless the enemy's crowd control resistance shrugs the taunt off (decision
/// 324). One random call for the target either way, and one more only when a taunt might be resisted, so battles
/// without a taunt, and enemies that resist nothing, keep their seeded results.
/// </summary>
internal sealed class EnemyTargeting(IRandom random)
{
    private readonly ChanceRolls _rolls = new(random);

    public Combatant ForAttack(BattleState battle)
    {
        var living = battle.LivingOn(BattleSide.Party);
        var taunting = living.Where(member => member.Statuses.Has(StatusKind.Taunt)).ToList();
        var targets = taunting.Count > 0 && !ResistsTaunt(battle.CurrentActor) ? taunting : living;
        return targets[random.NextBelow(targets.Count)];
    }

    // A taunt is self-granted, so the enemy rolls at its choice, not at the grant (I2).
    private bool ResistsTaunt(Combatant enemy) =>
        _rolls.Succeeds(enemy.CurrentStats.CrowdControlResistancePermille);
}
