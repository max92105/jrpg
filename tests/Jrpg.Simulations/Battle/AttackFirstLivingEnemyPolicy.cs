using Jrpg.Core.Battle;

namespace Jrpg.Simulations.Battle;

/// <summary>The simplest party behavior: always attack the first living enemy. A baseline, not how a player plays.</summary>
internal static class AttackFirstLivingEnemyPolicy
{
    public static BattleCommand ChooseCommand(BattleState battle) => new AttackCommand(battle.LivingOn(BattleSide.Enemies)[0]);
}
