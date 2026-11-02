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
/// item its message as one page, as does a quest step that changed the log (decision 387). A rest plays through
/// <see cref="RestPlayback"/>: an inn's rest asks first, with its price, then the beat before its message (decisions
/// 257, 382). A cutscene still covers the screen for its seconds, and Confirm or Cancel skips the cutscene's stills
/// (ADR 0016). A Forge or shop step asks the scene to open its screen, once (decision 353). The event's voice-over
/// plays through the voice channel until the event ends or is skipped (ADR 0018). The pages show through a
/// <see cref="MessageReading"/>, whose typing ticks the presenter asks; a found item, a recruit and a quest's change
/// sound as their line shows, a found item only when something was carried (decisions 421, 433); a warp from the map
/// sounds as its fade starts, never one out of a cutscene's black (decision 434), and a rest as
/// <see cref="RestPlayback"/> asks.
/// </summary>
internal sealed class EventPresenter
{
    private readonly ExplorationState _exploration;
    private readonly PlayerSettings _settings;
    private readonly UiMessages _messages;
    private readonly MessageReading _reading;
    private readonly StillRun _stills;
    private readonly RestPlayback _rest;
    private readonly ConfirmPromptView _questionView;
    private readonly VoiceChannel _voice;
    private readonly AskedSounds _sounds = new();

    // The fade for warps (placeholder timing).
    private readonly ScreenFade _warpFade = new(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.08), TimeSpan.FromSeconds(0.25));

    // The Forge or shop step its screen was opened for, so it opens once per step: a second one right after it, or one
    // an arrival event starts in the same update, is another step and opens it again. Null on any other step.
    private EventStep? _screenStepAskedFor;

    /// <param name="exploration">The map events to show.</param>
    /// <param name="look">Messages and fonts for the message box and the pages, and the question's textures.</param>
    /// <param name="settings">Read when a dialogue starts, so a text speed changed in the menu applies to the next one.</param>
    /// <param name="voice">
    /// Where the running event's voice-over is asked for, which also mutes the typing ticks.
    /// </param>
    public EventPresenter(ExplorationState exploration, EventLook look, PlayerSettings settings, VoiceChannel voice)
    {
        _exploration = exploration;
        _settings = settings;
        _messages = look.Text.Messages;
        _reading = new MessageReading(look.Text.Fonts, voice);
        _stills = new StillRun(exploration, new StillView(look.Text));
        _rest = new RestPlayback(look.Text.Messages);
        _questionView = new ConfirmPromptView(look.Text, look.Ui);
        _voice = voice;
    }

    /// <summary>True while an event runs or its fade plays: the player can't walk, interact or open the menu.</summary>
    public bool IsBusy => _exploration.IsEventRunning || _warpFade.Phase != FadePhase.Idle || _stills.IsLeadingOut;

    public IReadOnlyList<string> Sounds => _sounds.Files;

    /// <summary>Whether a cutscene still covers the screen.</summary>
    internal bool IsShowingStill => _stills.IsShowing;

    /// <summary>Whether a message (a dialogue page, a found item, a rest, a quest line) is on screen.</summary>
    internal bool IsShowingMessage => _reading.IsShowing;

    /// <summary>The page on screen, as the test reads it; null while none shows.</summary>
    internal string? ShownLine => _reading.ShownLine;

    /// <summary>The inn's question while it is open (decision 382); null otherwise.</summary>
    internal ConfirmPrompt? Question => _rest.Question;

    /// <summary>
    /// The Forge or shop step on the one update it first shows, so the scene opens its screen; null otherwise. Talking
    /// to the forge master or the shopkeeper again runs the event again, which opens it again.
    /// </summary>
    public EventStep? OpeningStep { get; private set; }

    /// <summary>How black to draw over the map, 0..1.</summary>
    public float Blackness => Math.Max(_stills.Blackness, Math.Max(_warpFade.Blackness, _rest.Blackness));

    /// <exception cref="InvalidOperationException">The current step is of a type the game can't show yet.</exception>
    public void Update(TimeSpan delta, InputFrame input)
    {
        _sounds.Clear();
        _warpFade.Update(delta);
        _rest.Advance(delta);
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
        FollowScreenStep(step);
        switch (step)
        {
            case null or OpenForgeStep or OpenShopStep:
                break;
            case WarpStep:
                ShowWarp();
                break;
            case DialogueStep or GiveItemStep or RecruitStep or StartQuestStep or AdvanceQuestStep or CompleteQuestStep:
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

    /// <summary>
    /// Draws over the map: a still, or the message being read, the inn's question while it is open, then the fade over
    /// everything.
    /// </summary>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        _stills.Draw(batch, textures);

        _reading.Draw(batch, textures);

        if (_rest.Question is { } question)
        {
            _questionView.Draw(batch, textures, question, 1f);
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

    // The Forge and a shop show themselves over the map: the step only asks for its screen, on its first update.
    private void FollowScreenStep(EventStep? step)
    {
        var screenStep = step is OpenForgeStep or OpenShopStep ? step : null;
        OpeningStep = screenStep is not null && !ReferenceEquals(screenStep, _screenStepAskedFor) ? screenStep : null;
        _screenStepAskedFor = screenStep;
    }

    // The step's first update starts the beat, or puts the inn's question up; the question takes the presses until it
    // is answered, and the page shows once the screen is back, as any page does. A decline continues the event
    // unrested.
    private void ShowRest(EventStep step, TimeSpan delta, InputFrame input)
    {
        if (!_rest.IsStarted)
        {
            _rest.Start(_exploration.CurrentInnStay, _sounds);
            return;
        }

        if (_rest.Question is not null)
        {
            _rest.Answer(input, _sounds);
            return;
        }

        if (_rest.IsFading)
        {
            return;
        }

        if (_rest.IsDeclined)
        {
            _rest.Finish();
            _exploration.ContinueEvent();
            return;
        }

        ShowPages(step, delta, input);
        if (!_reading.IsShowing)
        {
            _rest.Finish();
        }
    }

    private void ShowWarp()
    {
        if (_warpFade.Phase == FadePhase.Idle)
        {
            _warpFade.Start();
            _sounds.Ask(UiSoundFiles.Field.Warp);
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
        if (!_reading.IsShowing)
        {
            _reading.Show(StartReading(step));
            AskStepSound(step);
            return;
        }

        _reading.Update(delta, input);
        _sounds.AskAll(_reading.Sounds);
        if (!_reading.IsShowing)
        {
            _exploration.ContinueEvent();
        }
    }

    private void AskStepSound(EventStep step)
    {
        var file = step switch
        {
            GiveItemStep when _exploration.CurrentItemReceipt!.Added > 0 => UiSoundFiles.Field.ItemGot,
            RecruitStep when _exploration.CurrentRecruitment!.HasJoined => UiSoundFiles.Field.Recruit,
            StartQuestStep or AdvanceQuestStep => UiSoundFiles.Field.QuestUpdated,
            CompleteQuestStep => UiSoundFiles.Field.QuestCompleted,
            _ => null,
        };
        if (file is not null)
        {
            _sounds.Ask(file);
        }
    }

    private SpeechPlayback StartReading(EventStep step) => step switch
    {
        DialogueStep dialogue => new SpeechPlayback(dialogue.Speech, _settings.CharacterTime),

        // Shown whole: there's nothing to discover in it letter by letter. Core sets a receipt on every give-item step
        // and a recruitment on every recruit step, and a quest change on every quest step that waits.
        GiveItemStep => new SpeechPlayback(new Speech([FoundItemText.Of(_exploration.CurrentItemReceipt!, _messages)]), TimeSpan.Zero),
        RecruitStep => new SpeechPlayback(new Speech([RecruitText.Of(_exploration.CurrentRecruitment!, _messages)]), TimeSpan.Zero),
        StartQuestStep or AdvanceQuestStep or CompleteQuestStep => new SpeechPlayback(new Speech([QuestNoticeText.Of(_exploration.CurrentQuestChange!, _messages)]), TimeSpan.Zero),
        RestStep => new SpeechPlayback(new Speech([_messages.Text(_rest.LineId)]), TimeSpan.Zero),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, "Only dialogue, found items, recruits, rests and quest changes have pages."),
    };
}
