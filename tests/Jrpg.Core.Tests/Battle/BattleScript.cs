using Jrpg.Core.Battle;

namespace Jrpg.Core.Tests.Battle;

/// <summary>Plays a battle out without drawing from its random source, so a test's scripted values only cover the turn order.</summary>
internal static class BattleScript
{
    /// <summary>Every combatant attacks the first living one on the other side until the battle ends.</summary>
    public static void AttackUntilOver(BattleState battle)
    {
        while (!battle.IsOver)
        {
            var otherSide = battle.Phase == BattlePhase.PartyTurn ? BattleSide.Enemies : BattleSide.Party;
            battle.Execute(new AttackCommand(battle.LivingOn(otherSide)[0]));
        }
    }
}
