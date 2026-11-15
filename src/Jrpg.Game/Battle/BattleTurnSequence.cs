using Jrpg.Core.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// Whose turn the battle screen shows and what happens next (ADR 0013): a party actor steps forward and waits for a
/// command, an enemy pauses in place (decision 643) and asks Core's tactics; the action winds up and is executed at its
/// impact (ADR 0013 amended, decision 644) and the screen holds; then the party actor steps back (an enemy pauses
/// again), or, after a quick skill, waits forward for the next command. Time left over at the end of a stage carries
/// into the next one.
/// </summary>
internal sealed class BattleTurnSequence
{
    private readonly BattleState _battle;
    private readonly EnemyTactics _tactics;
    private readonly UiMessages _messages;
    private readonly IReadOnlyDictionary<string, EffectStripDefinition> _strips;
    private TimeSpan _timeInStage;

    // The command winding up, executed when its wind-up ends; null otherwise.
    private BattleCommand? _windingUp;

    /// <param name="battle">The battle it plays.</param>
    /// <param name="tactics">What enemies do on their turn.</param>
    /// <param name="messages">How the plaque writes what an actor does.</param>
    /// <param name="strips">The effect strips' records (ADR 0048), which name an attacker's swing.</param>
    public BattleTurnSequence(
        BattleState battle, EnemyTactics tactics, UiMessages messages, IReadOnlyDictionary<string, EffectStripDefinition> strips)
    {
        _battle = battle;
        _tactics = tactics;
        _messages = messages;
        _strips = strips;
        Actor = battle.CurrentActor;
        if (battle.IsOver)
        {
            // A battle lost at the start (decision 97) has no turn to play: it goes straight to its end screen.
            Stage = TurnStage.Over;
        }
    }

    public TurnStage Stage { get; private set; } = TurnStage.StepForward;

    /// <summary>Which turn the screen shows, counting from 1; with <see cref="Stage"/>, says when the screen must change.</summary>
    public int Turn { get; private set; } = 1;

    /// <summary>The last action's playback; null until someone acts. The screen holds for its beats.</summary>
    public ActionPlayback? Playback { get; private set; }

    /// <summary>The last action's wind-up; null until someone acts, kept until the next action's.</summary>
    public ActionWindUp? WindUp { get; private set; }

    public bool IsOver => Stage == TurnStage.Over;

    public bool IsAwaitingCommand => Stage == TurnStage.AwaitingCommand;

    /// <summary>The combatant acting on screen: the one who acted until the next turn steps forward, unlike Core's current actor.</summary>
    public Combatant Actor { get; private set; }

    /// <summary>
    /// How far <paramref name="combatant"/>, a party actor, has stepped out of the formation, 0 in place to 1 fully
    /// out; an enemy acts in place, always 0 (decision 643).
    /// </summary>
    public float StepShare(Combatant combatant)
    {
        if (combatant != Actor || combatant.Side != BattleSide.Party)
        {
            return 0;
        }

        return Stage switch
        {
            TurnStage.StepForward => (float)(_timeInStage / BattlePacing.Step),
            TurnStage.AwaitingCommand or TurnStage.WindingUp or TurnStage.Holding => 1,
            TurnStage.StepBack => 1 - (float)(_timeInStage / BattlePacing.Step),
            _ => 0,
        };
    }

    /// <summary>How visible <paramref name="combatant"/> is: 1 standing, fading while their fall plays, 0 once gone.</summary>
    public float OpacityOf(Combatant combatant) => combatant.IsAlive ? 1 : Playback?.FallOpacityOf(combatant) ?? 0;

    /// <summary>Plays the turn on; waiting for a command or the end of the battle stops the clock.</summary>
    public void Update(TimeSpan delta)
    {
        // The last action keeps playing (numbers hopping, the fallen fading) even while a command is awaited.
        Playback?.Update(delta);
        WindUp?.Update(delta);
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

        StartWindUp(command);
    }

    private TimeSpan? DurationOf(TurnStage stage) => stage switch
    {
        TurnStage.StepForward or TurnStage.StepBack => BattlePacing.Step,
        TurnStage.WindingUp => WindUp!.Duration,
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
                StartWindUp(_tactics.ChooseCommand(_battle));
                break;
            case TurnStage.WindingUp:
                Act(_windingUp!);
                break;
            case TurnStage.Holding when KeepsTurn:
                Stage = TurnStage.AwaitingCommand;
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

    // Core goes on with the same party member, after a quick skill (decision 202) or two of their turns in a row: they
    // stay forward for the next command.
    private bool KeepsTurn => _battle.Phase == BattlePhase.PartyTurn && _battle.CurrentActor == Actor;

    // An action with nothing to show before it lands (Defend, Move, Run) executes at once, as before the wind-up.
    private void StartWindUp(BattleCommand command)
    {
        WindUp = ActionWindUp.Of(Actor, command, _messages, _strips);
        if (WindUp.Duration == TimeSpan.Zero)
        {
            Act(command);
            return;
        }

        _windingUp = command;
        Stage = TurnStage.WindingUp;
    }

    private void Act(BattleCommand command)
    {
        _windingUp = null;
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
