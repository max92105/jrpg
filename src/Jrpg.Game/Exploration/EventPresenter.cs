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
/// player while the screen is black, and fades back in (legacy <c>GameManager.WarpAsync</c>); a found item shows its
/// message until Confirm.
/// </summary>
internal sealed class EventPresenter
{
    private readonly ExplorationState _exploration;
    private readonly UiMessages _messages;
    private readonly DialogueBox _messageBox;

    // Legacy SceneManager.TransitionAsync for warps.
    private readonly ScreenFade _warpFade = new(TimeSpan.FromSeconds(0.25), TimeSpan.FromSeconds(0.08), TimeSpan.FromSeconds(0.25));

    private bool _isMessageShown;

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
        switch (_exploration.CurrentEventStep)
        {
            case null:
                break;
            case WarpStep:
                ShowWarp();
                break;
            case GiveItemStep:
                ShowFoundItem(input);
                break;
            case var step:
                throw new InvalidOperationException($"The game can't show a {step.GetType().Name} yet.");
        }
    }

    /// <summary>Draws over the map: the message being read, then the fade over everything.</summary>
    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        if (_isMessageShown)
        {
            _messageBox.Draw(batch, textures);
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

    // The text is built once, on the step's first update; the press that started the event came an update earlier, so
    // it can't also close the message.
    private void ShowFoundItem(InputFrame input)
    {
        if (!_isMessageShown)
        {
            // Core sets a receipt on every give-item step.
            _messageBox.Show(FoundItemText.Of(_exploration.CurrentItemReceipt!, _messages));
            _isMessageShown = true;
        }
        else if (input.IsPressed(GameButton.Confirm))
        {
            _isMessageShown = false;
            _exploration.ContinueEvent();
        }
    }
}
