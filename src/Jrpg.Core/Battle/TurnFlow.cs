using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// How a battle moves from one turn to the next once an action has resolved: the actor's upkeep, the outcome check, and
/// the advance to the next actor, whose start ticks run first, skipping one they felled or a stunned one (ADR 0019, ADR
/// 0021). It returns the phase the battle is in after, and reports what happened as events.
/// </summary>
internal sealed class TurnFlow(IReadOnlyList<Combatant> combatants, TurnOrder turnOrder)
{
    public static BattlePhase PhaseOf(Combatant actor) =>
        actor.Side == BattleSide.Party ? BattlePhase.PartyTurn : BattlePhase.EnemyTurn;

    /// <summary>The actor's turn ends: upkeep (none on an extra turn), the outcome, then the next actor.</summary>
    public BattlePhase EndTurn(Combatant actor, List<BattleEvent> events)
    {
        RunUpkeep(actor, events);
        return OutcomeOf(events) ?? AdvanceToNextActor(events);
    }

    /// <summary>
    /// The actor keeps the turn after a quick action (decision 202): no upkeep, no advance, but a battle it won or lost
    /// still ends.
    /// </summary>
    public BattlePhase KeepTurn(Combatant actor, List<BattleEvent> events) => OutcomeOf(events) ?? PhaseOf(actor);

    // A readied extra turn has no upkeep, so it can't double the per-turn gains or the countdown (question 214).
    private void RunUpkeep(Combatant holder, List<BattleEvent> events)
    {
        if (turnOrder.IsExtraTurn)
        {
            return;
        }

        TurnUpkeep.TickStatuses(holder, StatusTiming.EndOfTurn, combatants, events);
        if (holder.IsAlive)
        {
            TurnUpkeep.ApplyPerTurnGains(holder, events);
        }

        // Runs even when the poison felled the holder: its own statuses are cleared, but the others' fresh marks still
        // need clearing.
        TurnUpkeep.CountDownStatuses(holder, combatants, events);
    }

    // Defeat is checked first, as in the demo: if both sides fall in one turn, the party loses.
    private BattlePhase? OutcomeOf(List<BattleEvent> events)
    {
        BattlePhase? outcome =
            !AnyoneStandsOn(BattleSide.Party) ? BattlePhase.Defeat
            : !AnyoneStandsOn(BattleSide.Enemies) ? BattlePhase.Victory
            : null;
        if (outcome is { } ended)
        {
            events.Add(new BattleEnded(ended));
        }

        return outcome;
    }

    // Each turn starts with its holder's start ticks, then a stun skips it (decision 804): a holder the ticks felled,
    // or a stunned one, loses the turn inside the same Execute, so the screen never sees it as the actor. A stunned
    // turn still runs its upkeep, so the stun counts down and the loop ends; a lost extra turn has none, but it is
    // always followed by a cycle turn, which counts down (ADR 0021).
    private BattlePhase AdvanceToNextActor(List<BattleEvent> events)
    {
        while (true)
        {
            turnOrder.Advance();
            if (StartTurn(events) is { } ended)
            {
                return ended;
            }

            var actor = turnOrder.Current;
            if (!actor.IsAlive)
            {
                continue;
            }

            if (actor.Statuses.FirstWith(StatusPartKind.SkipTurn) is not { } stun)
            {
                return PhaseOf(actor);
            }

            events.Add(new TurnSkipped(actor, stun));
            RunUpkeep(actor, events);
            if (OutcomeOf(events) is { } outcome)
            {
                return outcome;
            }
        }
    }

    // A readied extra turn has no start ticks, as it has no upkeep; a tick that ends the battle ends it at once.
    private BattlePhase? StartTurn(List<BattleEvent> events)
    {
        if (turnOrder.IsExtraTurn)
        {
            return null;
        }

        TurnUpkeep.TickStatuses(turnOrder.Current, StatusTiming.StartOfTurn, combatants, events);
        return OutcomeOf(events);
    }

    private bool AnyoneStandsOn(BattleSide side) => combatants.Any(combatant => combatant.Side == side && combatant.IsAlive);
}
