using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Audio;
using Jrpg.Game.Dialogue;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Shows the running map event's current step and continues it once shown (ADR 0006): a warp fades to black, moves the
/// player while the screen is black, and fades back in; a dialogue shows its lines one page per Confirm, and a found
/// item its message as one page. A rest fades to black and back before its message (decision 257). A cutscene still
/// covers the screen for its seconds, and Confirm or Cancel skips the cutscene's stills (ADR 0016). A Forge step asks
/// the scene to open the Forge, once (decision 353). The event's voice-over plays through the voice channel until the
/// event ends or is skipped (ADR 0018).
/// </summary>
internal sealed class EventPresenter
{
    private readonly ExplorationState _exploration;
    private readonly PlayerSettings _settings;
    private readonly UiMessages _messages;
    private readonly DialogueBox _messageBox;
    private readonly StillRun _stills;
    private readonly VoiceChannel _voice;

    // The fade for warps (placeholder timing).
    private readonly ScreenFade _warpFade = new(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.08), TimeSpan.FromSeconds(0.25));

    // The inn's beat (decision 257): out, a second of black, back in, then the party rests (placeholder timing).
    private readonly ScreenFade _restFade = new(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(0.5));

    // Whether the current rest step has started its fade, so its page's own updates don't start it again.
    private bool _isRestFadeStarted;

    // The Forge step the Forge was opened for, so it opens once per step: a second one right after it, or one an
    // arrival event starts in the same update, is another step and opens it again. Null on any other step.
    private OpenForgeStep? _forgeStepAskedFor;

    // The pages of the step being read; null when no message is on screen.
    private SpeechPlayback? _reading;

    /// <param name="exploration">The map events to show.</param>
    /// <param name="text">Messages and fonts for the message box.</param>
    /// <param name="settings">Read when a dialogue starts, so a text speed changed in the menu applies to the next one.</param>
    /// <param name="voice">Where the running event's voice-over is asked for.</param>
    public EventPresenter(ExplorationState exploration, ScreenText text, PlayerSettings settings, VoiceChannel voice)
    {
        _exploration = exploration;
        _settings = settings;
        _messages = text.Messages;
        _messageBox = new DialogueBox(text.Fonts);
        _stills = new StillRun(exploration, new StillView(text));
        _voice = voice;
    }

    /// <summary>True while an event runs or its fade plays: the player can't walk, interact or open the menu.</summary>
    public bool IsBusy => _exploration.IsEventRunning || _warpFade.Phase != FadePhase.Idle || _stills.IsLeadingOut;

    /// <summary>Whether a cutscene still covers the screen.</summary>
    internal bool IsShowingStill => _stills.IsShowing;

    /// <summary>Whether a message (a dialogue page, a found item, a rest) is on screen.</summary>
    internal bool IsShowingMessage => _reading is not null;

    /// <summary>
    /// True on the one update a Forge step first shows: the scene opens the Forge. Talking to the forge master again
    /// runs the event again, which opens it again.
    /// </summary>
    public bool IsOpeningForge { get; private set; }

    /// <summary>How black to draw over the map, 0..1.</summary>
    public float Blackness => Math.Max(_stills.Blackness, Math.Max(_warpFade.Blackness, _restFade.Blackness));

    /// <exception cref="InvalidOperationException">The current step is of a type the game can't show yet.</exception>
    public void Update(TimeSpan delta, InputFrame input)
    {
        _warpFade.Update(delta);
        _restFade.Update(delta);
        if (_stills.IsLeadingOut)
        {
            _stills.AdvanceLeadOut(delta);
            if (_stills.IsLeadingOut)
            {
                FollowStartedVoice();
                return;
            }

            // The black just ended: what follows shows this same update, so no frame of the bare map comes first.
            StayBlackIntoAWarp();
        }

        var step = _exploration.CurrentEventStep;
        FollowForgeStep(step);
        switch (step)
        {
            case null or OpenForgeStep:
                break;
            case WarpStep:
                ShowWarp();
                break;
            case DialogueStep or GiveItemStep or RecruitStep:
                ShowPages(step, delta, input);
                break;
            case StillStep still:
                ShowStill(still, delta, input);
                break;
            case RestStep:
                ShowRest(step, delta, input);
                break;
            default:
                throw new InvalidOperationException($"The game can't show a {step.GetType().Name} yet.");
        }

        FollowStartedVoice();
    }

    /// <summary>Draws over the map: a still, or the message being read, then the fade over everything.</summary>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        _stills.Draw(batch, textures);

        if (_reading is not null)
        {
            _messageBox.Draw(batch, textures, _reading);
        }

        var blackness = Blackness;
        if (blackness > 0)
        {
            batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.Black * blackness);
        }
    }

    // A skip ends the run at once: what follows shows this same update, already black if it's a warp.
    private void ShowStill(StillStep still, TimeSpan delta, InputFrame input)
    {
        _stills.Show(still, delta, input);
        if (!_stills.IsRunning)
        {
            StayBlackIntoAWarp();
        }
    }

    // A warp right after the cutscene's black starts its fade already black, so the map it leaves never shows again.
    private void StayBlackIntoAWarp()
    {
        if (_exploration.CurrentEventStep is WarpStep && _warpFade.Phase == FadePhase.Idle)
        {
            _warpFade.StartAtBlack();
        }
    }

    // The Forge shows itself over the map: the step only asks for it, on its first update.
    private void FollowForgeStep(EventStep? step)
    {
        var forgeStep = step as OpenForgeStep;
        IsOpeningForge = forgeStep is not null && !ReferenceEquals(forgeStep, _forgeStepAskedFor);
        _forgeStepAskedFor = forgeStep;
    }

    // The step's first update starts the fade; its message shows once the screen is back, as any page does.
    private void ShowRest(EventStep step, TimeSpan delta, InputFrame input)
    {
        if (!_isRestFadeStarted)
        {
            _restFade.Start();
            _isRestFadeStarted = true;
            return;
        }

        if (_restFade.Phase != FadePhase.Idle)
        {
            return;
        }

        ShowPages(step, delta, input);
        if (_reading is null)
        {
            _isRestFadeStarted = false;
        }
    }

    private void ShowWarp()
    {
        if (_warpFade.Phase == FadePhase.Idle)
        {
            _warpFade.Start();
        }
        else if (_warpFade.Phase == FadePhase.Holding)
        {
            _exploration.ContinueEvent();
        }
    }

    // After the step: a skip or the event's end this update silences the voice at once.
    private void FollowStartedVoice()
    {
        if (_exploration.StartedVoice is { } voice)
        {
            _voice.Play(voice.File);
        }
        else
        {
            _voice.Stop();
        }
    }

    // The pages are built once, on the step's first update; the press that started the event came an update earlier, so
    // it can't also complete the first page.
    private void ShowPages(EventStep step, TimeSpan delta, InputFrame input)
    {
        if (_reading is null)
        {
            _reading = StartReading(step);
            _messageBox.Show(_reading);
            return;
        }

        // Confirm is judged on the page as the player last saw it, so a press on the update the last character would
        // appear completes the page rather than turning it (decision 54).
        if (!input.IsPressed(GameButton.Confirm))
        {
            _reading.Update(delta);
            return;
        }

        var shownPage = _reading.PageIndex;
        _reading.Confirm();
        if (_reading.IsFinished)
        {
            _reading = null;
            _exploration.ContinueEvent();
        }
        else if (_reading.PageIndex != shownPage)
        {
            _messageBox.Show(_reading);
        }
    }

    private SpeechPlayback StartReading(EventStep step) => step switch
    {
        DialogueStep dialogue => new SpeechPlayback(dialogue.Speech, _settings.CharacterTime),

        // Shown whole: there's nothing to discover in it letter by letter. Core sets a receipt on every give-item step
        // and a recruitment on every recruit step.
        GiveItemStep => new SpeechPlayback(new Speech([FoundItemText.Of(_exploration.CurrentItemReceipt!, _messages)]), TimeSpan.Zero),
        RecruitStep => new SpeechPlayback(new Speech([RecruitText.Of(_exploration.CurrentRecruitment!, _messages)]), TimeSpan.Zero),
        RestStep => new SpeechPlayback(new Speech([_messages.Text(MessageIds.PartyRests)]), TimeSpan.Zero),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, "Only dialogue, found items, recruits and rests have pages."),
    };
}
