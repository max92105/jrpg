using Jrpg.Core.Randomness;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// Chooses what an enemy does on its turn: attack a random living party member, or a random taunting one while any
/// member taunts (question 209). One random call either way, so battles without a taunt keep their seeded results.
/// </summary>
public sealed class EnemyTactics(IRandom random)
{
    /// <exception cref="InvalidOperationException">It isn't an enemy's turn.</exception>
    public BattleCommand ChooseCommand(BattleState battle)
    {
        if (battle.Phase != BattlePhase.EnemyTurn)
        {
            throw new InvalidOperationException($"Enemy tactics only choose on an enemy turn (phase is {battle.Phase}).");
        }

        var living = battle.LivingOn(BattleSide.Party);
        var taunting = living.Where(member => member.Statuses.Has(StatusKind.Taunt)).ToList();
        var targets = taunting.Count > 0 ? taunting : living;
        return new AttackCommand(targets[random.NextBelow(targets.Count)]);
    }
}
