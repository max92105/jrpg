using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

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
    private readonly UiMessages _messages;
    private TimeSpan _timeInStage;

    /// <param name="battle">The battle it plays.</param>
    /// <param name="tactics">What enemies do on their turn.</param>
    /// <param name="messages">How the banner writes what an actor does.</param>
    public BattleTurnSequence(BattleState battle, EnemyTactics tactics, UiMessages messages)
    {
        _battle = battle;
        _tactics = tactics;
        _messages = messages;
        Actor = battle.CurrentActor;
    }

    public TurnStage Stage { get; private set; } = TurnStage.StepForward;

    /// <summary>Which turn the screen shows, counting from 1; with <see cref="Stage"/>, says when the screen must change.</summary>
    public int Turn { get; private set; } = 1;

    /// <summary>The last action's playback; null until someone acts. The screen holds for its beats.</summary>
    public ActionPlayback? Playback { get; private set; }

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

    private TimeSpan? DurationOf(TurnStage stage) => stage switch
    {
        TurnStage.StepForward or TurnStage.StepBack => BattlePacing.Step,
        TurnStage.Holding => Playback?.Duration ?? TimeSpan.Zero,
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
        Playback = ActionPlayback.Start(_battle.Execute(command), _messages);
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
