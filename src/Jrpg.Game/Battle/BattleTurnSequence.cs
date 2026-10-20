using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>
/// Whose turn the battle screen shows and what happens next (ADR 0013): the actor steps forward, a party member waits for
/// a command while an enemy asks Core's tactics, the command is executed and the screen holds, then the actor steps back.
/// Time left over at the end of a stage carries into the next one.
/// </summary>
internal sealed class BattleTurnSequence
{
    private readonly BattleState _battle;
    private readonly EnemyTactics _tactics;
    private TimeSpan _timeInStage;
    private TimeSpan _hold;

    public BattleTurnSequence(BattleState battle, EnemyTactics tactics)
    {
        _battle = battle;
        _tactics = tactics;
        Actor = battle.CurrentActor;
    }

    public TurnStage Stage { get; private set; } = TurnStage.StepForward;

    /// <summary>Which turn the screen shows, counting from 1; with <see cref="Stage"/>, says when the screen must change.</summary>
    public int Turn { get; private set; } = 1;

    public bool IsOver => Stage == TurnStage.Over;

    public bool IsAwaitingCommand => Stage == TurnStage.AwaitingCommand;

    /// <summary>The combatant acting on screen: the one who acted until the next turn steps forward, unlike Core's current actor.</summary>
    public Combatant Actor { get; private set; }

    /// <summary>How far <paramref name="combatant"/> has stepped out of the formation, 0 in place to 1 fully out.</summary>
    public float StepShare(Combatant combatant)
    {
        if (combatant != Actor)
        {
            return 0;
        }

        return Stage switch
        {
            TurnStage.StepForward => (float)(_timeInStage / BattlePacing.Step),
            TurnStage.AwaitingCommand or TurnStage.Holding => 1,
            TurnStage.StepBack => 1 - (float)(_timeInStage / BattlePacing.Step),
            _ => 0,
        };
    }

    /// <summary>Plays the turn on; waiting for a command or the end of the battle stops the clock.</summary>
    public void Update(TimeSpan delta)
    {
        if (DurationOf(Stage) is null)
        {
            return;
        }

        _timeInStage += delta;
        while (DurationOf(Stage) is { } duration && _timeInStage >= duration)
        {
            _timeInStage -= duration;
            EndStage();
        }
    }

    /// <summary>The acting party member carries out <paramref name="command"/>.</summary>
    /// <exception cref="InvalidOperationException">No party member is waiting for a command.</exception>
    public void Submit(BattleCommand command)
    {
        if (!IsAwaitingCommand)
        {
            throw new InvalidOperationException($"No party member is waiting for a command ({Stage}).");
        }

        Act(command);
    }

    // Defend and Run land on nobody, so nothing holds the screen (B5b paces each event instead).
    private static TimeSpan HoldFor(IReadOnlyList<BattleEvent> events) =>
        events.Any(battleEvent => battleEvent is CombatantDefeated) ? BattlePacing.DefeatHold
        : events.Any(battleEvent => battleEvent is EffectApplied) ? BattlePacing.ImpactHold
        : TimeSpan.Zero;

    private TimeSpan? DurationOf(TurnStage stage) => stage switch
    {
        TurnStage.StepForward or TurnStage.StepBack => BattlePacing.Step,
        TurnStage.Holding => _hold,
        _ => null,
    };

    private void EndStage()
    {
        switch (Stage)
        {
            case TurnStage.StepForward when _battle.Phase == BattlePhase.PartyTurn:
                Stage = TurnStage.AwaitingCommand;
                _timeInStage = TimeSpan.Zero; // The wait is the player's; leftover time doesn't count toward the hold.
                break;
            case TurnStage.StepForward:
                Act(_tactics.ChooseCommand(_battle));
                break;
            case TurnStage.Holding:
                Stage = TurnStage.StepBack;
                break;
            case TurnStage.StepBack:
                StartNextTurn();
                break;
            default:
                throw new InvalidOperationException($"The {Stage} stage doesn't end on its own.");
        }
    }

    private void Act(BattleCommand command)
    {
        _hold = HoldFor(_battle.Execute(command));
        Stage = TurnStage.Holding;
    }

    private void StartNextTurn()
    {
        if (_battle.IsOver)
        {
            Stage = TurnStage.Over;
            return;
        }

        Actor = _battle.CurrentActor;
        Turn++;
        Stage = TurnStage.StepForward;
    }
}
