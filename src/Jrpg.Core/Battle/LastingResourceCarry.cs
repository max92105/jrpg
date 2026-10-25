namespace Jrpg.Core.Battle;

/// <summary>
/// The end of a battle for lasting resources (ADR 0024): each party member keeps what the battle left of them, changed
/// by the resource's per-battle-fought gain.
/// </summary>
internal static class LastingResourceCarry
{
    /// <param name="battle">A battle that is over, whatever its outcome.</param>
    public static void CarryBack(BattleState battle)
    {
        foreach (var combatant in battle.Combatants)
        {
            if (combatant.Member is not { } member)
            {
                continue;
            }

            foreach (var resource in member.Class.Resources.Where(resource => resource.IsLasting))
            {
                member.KeepLastingAmount(resource.Id, combatant.Resources.AmountOf(resource.Id) + resource.Gains.PerBattleFought);
            }
        }
    }
}
