using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;

namespace Jrpg.Core.Battle;

/// <summary>
/// A battle in progress. The only way to change it is <see cref="Execute"/>, one call per turn,
/// which returns what happened as battle events (ADR 0004).
/// </summary>
public sealed class BattleState
{
    private readonly TurnOrder _turnOrder;

    /// <exception cref="ArgumentException">A side has no living combatant, or a combatant is listed twice.</exception>
    public BattleState(IReadOnlyList<Combatant> combatants, IRandom random)
    {
        Combatants = [.. combatants];
        if (Combatants.Distinct().Count() != Combatants.Count)
        {
            throw new ArgumentException("A combatant can only be in a battle once.", nameof(combatants));
        }

        if (LivingOn(BattleSide.Party).Count == 0 || LivingOn(BattleSide.Enemies).Count == 0)
        {
            throw new ArgumentException("Both sides need at least one living combatant.", nameof(combatants));
        }

        _turnOrder = new TurnOrder(Combatants, random);
        Phase = TurnPhaseOf(CurrentActor);
    }

    public BattlePhase Phase { get; private set; }

    public bool IsOver => Phase is BattlePhase.Victory or BattlePhase.Defeat or BattlePhase.Escaped;

    public Combatant CurrentActor => _turnOrder.Current;

    public IReadOnlyList<Combatant> Combatants { get; }

    public IReadOnlyList<Combatant> LivingOn(BattleSide side) =>
        [.. Combatants.Where(combatant => combatant.Side == side && combatant.IsAlive)];

    /// <summary>The current actor carries out <paramref name="command"/>, then the turn ends.</summary>
    /// <returns>What happened, in order, starting with <see cref="ActionStarted"/>.</returns>
    /// <exception cref="InvalidOperationException">The battle is over, or the current actor can't give this command.</exception>
    /// <exception cref="ArgumentException">The command's target is not a legal target.</exception>
    public IReadOnlyList<BattleEvent> Execute(BattleCommand command)
    {
        if (IsOver)
        {
            throw new InvalidOperationException($"The battle is over ({Phase}).");
        }

        var actor = CurrentActor;
        var events = new List<BattleEvent> { new ActionStarted(actor, command) };
        switch (command)
        {
            case AttackCommand attack:
                Attack(actor, attack.Target, events);
                break;
            case DefendCommand:
                break;
            case FleeCommand:
                Flee(actor, events);
                break;
            default:
                throw new ArgumentException($"Unknown command {command.GetType().Name}.", nameof(command));
        }

        EndTurn(events);
        return events;
    }

    private static BattlePhase TurnPhaseOf(Combatant actor) =>
        actor.Side == BattleSide.Party ? BattlePhase.PartyTurn : BattlePhase.EnemyTurn;

    private void Attack(Combatant actor, Combatant target, List<BattleEvent> events)
    {
        if (!Combatants.Contains(target) || target.Side == actor.Side || !target.IsAlive)
        {
            throw new ArgumentException($"{target.Name} is not a living opponent of {actor.Name} in this battle.", nameof(target));
        }

        var damage = DamageFormula.Calculate(actor.Stats.Attack, target.Stats.Defense);
        events.Add(new EffectApplied(target, EffectKind.Damage, target.Vitals.TakeDamage(damage)));
        if (!target.IsAlive)
        {
            events.Add(new CombatantDefeated(target));
        }
    }

    private void Flee(Combatant actor, List<BattleEvent> events)
    {
        if (actor.Side != BattleSide.Party)
        {
            throw new InvalidOperationException("Only the party can flee.");
        }

        Phase = BattlePhase.Escaped;
        events.Add(new BattleEnded(Phase));
    }

    private void EndTurn(List<BattleEvent> events)
    {
        if (IsOver)
        {
            return;
        }

        // Defeat is checked first, as in the demo: if both sides fall in one turn, the party loses.
        if (LivingOn(BattleSide.Party).Count == 0)
        {
            Phase = BattlePhase.Defeat;
        }
        else if (LivingOn(BattleSide.Enemies).Count == 0)
        {
            Phase = BattlePhase.Victory;
        }
        else
        {
            _turnOrder.Advance();
            Phase = TurnPhaseOf(CurrentActor);
            return;
        }

        events.Add(new BattleEnded(Phase));
    }
}
