using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Game.Dialogue;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Shows the running map event's current step and continues it once shown (ADR 0006): a warp fades to black, moves the
/// player while the screen is black, and fades back in (legacy <c>GameManager.WarpAsync</c>); a dialogue shows its lines
/// one page per Confirm, and a found item its message as one page.
/// </summary>
internal sealed class EventPresenter
{
    // Developer decision 54; becomes the player's text speed with the Options page (P5).
    private static readonly TimeSpan _dialogueCharacterTime = TimeSpan.FromMilliseconds(30);

    private readonly ExplorationState _exploration;
    private readonly UiMessages _messages;
    private readonly DialogueBox _messageBox;

    // Legacy SceneManager.TransitionAsync for warps.
    private readonly ScreenFade _warpFade = new(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.08), TimeSpan.FromSeconds(0.25));

    // The pages of the step being read; null when no message is on screen.
    private SpeechPlayback? _reading;

    public EventPresenter(ExplorationState exploration, ScreenText text)
    {
        _exploration = exploration;
        _messages = text.Messages;
        _messageBox = new DialogueBox(text.Fonts);
    }

    /// <summary>True while an event runs or its fade plays: the player can't walk, interact or open the menu.</summary>
    public bool IsBusy => _exploration.IsEventRunning || _warpFade.Phase != FadePhase.Idle;

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
            default:
                throw new InvalidOperationException($"The game can't show a {step.GetType().Name} yet.");
        }
    }

    /// <summary>Draws over the map: the message being read, then the fade over everything.</summary>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
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
            _messageBox.Show(_reading.CurrentPage);
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
            _messageBox.Show(_reading.CurrentPage);
        }
    }

    private SpeechPlayback StartReading(EventStep step) => step switch
    {
        DialogueStep dialogue => new SpeechPlayback(dialogue.Speech.Lines, _dialogueCharacterTime),

        // Shown whole: there's nothing to discover in it letter by letter. Core sets a receipt on every give-item step.
        GiveItemStep => new SpeechPlayback([FoundItemText.Of(_exploration.CurrentItemReceipt!, _messages)], TimeSpan.Zero),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step, "Only dialogue and found items have pages."),
    };
}
