namespace Jrpg.Core.Battle;

/// <summary>The end of a combatant's own turn, before the battle's outcome is checked (ADR 0019).</summary>
internal static class TurnUpkeep
{
    public static void ApplyPerTurnGains(Combatant actor, List<BattleEvent> events)
    {
        foreach (var resource in actor.Resources.Definitions)
        {
            var change = actor.Resources.Change(resource.Id, resource.Gains.PerTurn);
            if (change != 0)
            {
                events.Add(new ResourceChanged(actor, resource.Id, change));
            }
        }
    }
}
