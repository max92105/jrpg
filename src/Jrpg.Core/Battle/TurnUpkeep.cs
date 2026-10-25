namespace Jrpg.Core.Battle;

/// <summary>The end of a combatant's own turn, before the battle's outcome is checked (ADR 0019, ADR 0021).</summary>
internal static class TurnUpkeep
{
    public static void ApplyPerTurnGains(Combatant actor, List<BattleEvent> events)
    {
        foreach (var resource in actor.Resources.Definitions)
        {
            actor.ChangeResource(resource.Id, resource.Gains.PerTurn, events);
        }
    }

    /// <summary>
    /// The actor's statuses count down, except one granted during this turn. Everyone else's statuses granted during it
    /// lose their fresh mark, so they start counting from their holder's next turn (ADR 0021).
    /// </summary>
    public static void CountDownStatuses(Combatant actor, IReadOnlyList<Combatant> combatants, List<BattleEvent> events)
    {
        foreach (var status in actor.Statuses.CountDown())
        {
            events.Add(new StatusEnded(actor, status));
        }

        foreach (var other in combatants.Where(combatant => combatant != actor))
        {
            other.Statuses.ClearFreshMarks();
        }
    }
}
