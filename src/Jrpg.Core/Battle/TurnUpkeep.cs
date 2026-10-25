using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>The end of a combatant's own turn, before the battle's outcome is checked (ADR 0019, ADR 0021).</summary>
internal static class TurnUpkeep
{
    /// <summary>
    /// The holder's Poison and Regen take or restore their power in HP, in the order applied; a fresh one ticks too,
    /// only its countdown waits. Poison isn't a hit, so it ignores defense and Exposed and sets off no hit trigger.
    /// </summary>
    public static void TickStatuses(Combatant holder, IReadOnlyList<Combatant> combatants, List<BattleEvent> events)
    {
        // A fall clears the statuses, so the loop walks a copy.
        foreach (var status in holder.Statuses.Active.ToList())
        {
            int? amount = status.Kind switch
            {
                StatusKind.Poison => holder.Vitals.TakeDamage(status.Power),
                StatusKind.Regen => holder.Vitals.RestoreHp(status.Power),
                _ => null,
            };
            if (amount is not { } ticked)
            {
                continue;
            }

            events.Add(new StatusTicked(holder, status.Kind, ticked));
            if (!holder.IsAlive)
            {
                EffectLanding.Fall(holder, combatants, events);
                return;
            }
        }
    }

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
