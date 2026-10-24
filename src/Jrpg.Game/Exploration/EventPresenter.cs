using FontStashSharp;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Data.Art;
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
/// item its message as one page. A cutscene still covers the screen for its seconds, and Confirm or Cancel skips the
/// cutscene's stills (ADR 0016).
/// </summary>
internal sealed class EventPresenter
{
    private readonly ExplorationState _exploration;
    private readonly PlayerSettings _settings;
    private readonly UiMessages _messages;
    private readonly DialogueBox _messageBox;
    private readonly SpriteFontBase _captionFont;

    // Subtitles at 32 px, their middle 120 px above the bottom (placeholder; K4 gives them a band).
    private const int CaptionTextPixels = 32;
    private const int CaptionMiddleAbove = 120;

    // The fade for warps (placeholder timing).
    private readonly ScreenFade _warpFade = new(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.08), TimeSpan.FromSeconds(0.25));

    // The pages of the step being read; null when no message is on screen.
    private SpeechPlayback? _reading;

    // The still on screen and its subtitle, measured when it starts; null when no still shows.
    private StillPlayback? _still;
    private MeasuredText? _caption;

    /// <param name="exploration">The map events to show.</param>
    /// <param name="text">Messages and fonts for the message box.</param>
    /// <param name="settings">Read when a dialogue starts, so a text speed changed in the menu applies to the next one.</param>
    public EventPresenter(ExplorationState exploration, ScreenText text, PlayerSettings settings)
    {
        _exploration = exploration;
        _settings = settings;
        _messages = text.Messages;
        _messageBox = new DialogueBox(text.Fonts);
        _captionFont = text.Fonts.FontAt(CaptionTextPixels);
    }

    /// <summary>True while an event runs or its fade plays: the player can't walk, interact or open the menu.</summary>
    public bool IsBusy => _exploration.IsEventRunning || _warpFade.Phase != FadePhase.Idle;

    /// <summary>Whether a cutscene still covers the screen.</summary>
    internal bool IsShowingStill => _still is not null;

    /// <summary>How black to draw over the map, 0..1.</summary>
    public float Blackness => _warpFade.Blackness;

    /// <exception cref="InvalidOperationException">The current step is of a type the game can't show yet.</exception>
    public void Update(TimeSpan delta, InputFrame input)
    {
        _warpFade.Update(delta);
        var step = _exploration.CurrentEventStep;
        switch (step)
        {
            case null:
                break;
            case WarpStep:
                ShowWarp();
                break;
            case DialogueStep or GiveItemStep:
                ShowPages(step, delta, input);
                break;
            case StillStep still:
                ShowStill(still, delta, input);
                break;
            default:
                throw new InvalidOperationException($"The game can't show a {step.GetType().Name} yet.");
        }
    }

    /// <summary>Draws over the map: a still, or the message being read, then the fade over everything.</summary>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        if (_still is not null)
        {
            DrawStill(batch, textures, _still);
        }

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

    // Begun on the step's first update; Confirm or Cancel runs the event past its stills, so the press can't also turn the
    // first page of a dialogue that follows.
    private void ShowStill(StillStep still, TimeSpan delta, InputFrame input)
    {
        if (_still is null)
        {
            BeginStill(still);
            return;
        }

        if (input.IsPressed(GameButton.Confirm) || input.IsPressed(GameButton.Cancel))
        {
            _still = null;
            _exploration.SkipEventPresentation();
            return;
        }

        _still.Update(delta);
        if (!_still.IsOver)
        {
            return;
        }

        _still = null;
        _exploration.ContinueEvent();

        // The next still starts now, so this frame draws it rather than one frame of the bare map.
        if (_exploration.CurrentEventStep is StillStep next)
        {
            BeginStill(next);
        }
    }

    private void BeginStill(StillStep still)
    {
        _still = new StillPlayback(still);
        _caption = still.Caption is { } caption ? MeasuredText.Of(_captionFont, caption) : null;
    }

    // Black under the still, so the map never shows through while it fades; the subtitle fades with it.
    private void DrawStill(SpriteBatch batch, TextureLibrary textures, StillPlayback still)
    {
        var tint = Color.White * still.Opacity;
        batch.Draw(textures.Pixel, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.Black);
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Cinematics, still.Step.ImageFile), tint);
        if (_caption is { } caption)
        {
            var position = new Vector2((VirtualScreen.Width - caption.Width) / 2, VirtualScreen.Height - CaptionMiddleAbove - (_captionFont.LineHeight / 2f));
            batch.DrawString(_captionFont, caption.Text, position, UiTheme.TextPrimary * still.Opacity);
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

        // Shown whole: there's nothing to discover in it letter by letter. Core sets a receipt on every give-item step.
        GiveItemStep => new SpeechPlayback(new Speech([FoundItemText.Of(_exploration.CurrentItemReceipt!, _messages)]), TimeSpan.Zero),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, "Only dialogue and found items have pages."),
    };
}
