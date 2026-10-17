using Jrpg.Core.Randomness;

namespace Jrpg.Core.Battle;

/// <summary>Chooses what an enemy does on its turn: attack a random living party member.</summary>
internal sealed class EnemyTactics(IRandom random)
{
    /// <exception cref="InvalidOperationException">It isn't an enemy's turn.</exception>
    public BattleCommand ChooseCommand(BattleState battle)
    {
        if (battle.Phase != BattlePhase.EnemyTurn)
        {
            throw new InvalidOperationException($"Enemy tactics only choose on an enemy turn (phase is {battle.Phase}).");
        }

        var targets = battle.LivingOn(BattleSide.Party);
        return new AttackCommand(targets[random.NextBelow(targets.Count)]);
    }
}
