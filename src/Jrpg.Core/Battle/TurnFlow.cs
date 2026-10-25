using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// How a battle moves from one turn to the next once an action has resolved: the actor's upkeep, the outcome check, and
/// the advance to the next actor, skipping a stunned one (ADR 0019, ADR 0021). It returns the phase the battle is in
/// after, and reports what happened as events.
/// </summary>
internal sealed class TurnFlow(IReadOnlyList<Combatant> combatants, TurnOrder turnOrder)
{
    public static BattlePhase PhaseOf(Combatant actor) =>
        actor.Side == BattleSide.Party ? BattlePhase.PartyTurn : BattlePhase.EnemyTurn;

    /// <summary>The actor's turn ends: upkeep, then the outcome, then the next actor.</summary>
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

    private void RunUpkeep(Combatant holder, List<BattleEvent> events)
    {
        TurnUpkeep.ApplyPerTurnGains(holder, events);
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

    // A stunned combatant's turn is skipped inside the same Execute, so the screen never sees it as the actor. The
    // lost turn still runs its upkeep, so the stun counts down and the loop always ends (ADR 0021).
    private BattlePhase AdvanceToNextActor(List<BattleEvent> events)
    {
        turnOrder.Advance();
        while (turnOrder.Current.Statuses.Has(StatusKind.Stun))
        {
            var skipped = turnOrder.Current;
            events.Add(new TurnSkipped(skipped, StatusKind.Stun));
            RunUpkeep(skipped, events);
            if (OutcomeOf(events) is { } outcome)
            {
                return outcome;
            }

            turnOrder.Advance();
        }

        return PhaseOf(turnOrder.Current);
    }

    private bool AnyoneStandsOn(BattleSide side) => combatants.Any(combatant => combatant.Side == side && combatant.IsAlive);
}
