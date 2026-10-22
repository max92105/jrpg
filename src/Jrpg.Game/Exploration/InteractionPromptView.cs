using FontStashSharp;
using Jrpg.Core.Exploration;
using Jrpg.Core.Maps;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Above the object the player faces: the device's confirm key in a light key cap, then the event's prompt, in a small
/// dark pill that fades in and out (<see cref="PromptFade"/>).
/// </summary>
internal sealed class InteractionPromptView
{
    // The key's side margin inside its cap, and the space between the cap and the prompt's word.
    private const int KeyCapMargin = 6;
    private const int KeyGap = 8;

    private readonly SpriteFontBase _font;
    private readonly Func<PlacedObject, ObjectDefinition> _currentDefinitionOf;
    private readonly PromptFade _fade = new();
    private readonly PanelView _pill = new(UiTheme.PromptPanel);
    private readonly PanelView _keyCap = new(UiTheme.KeyCap);
    private InteractionTarget? _measured;
    private InputDevice _device;
    private MeasuredText _key;
    private MeasuredText _prompt;
    private Point _boxSize;
    private int _keyCapWidth;

    /// <param name="text">The fonts.</param>
    /// <param name="currentDefinitionOf">What an object is now, after any swap, for the prompt's width over it.</param>
    public InteractionPromptView(ScreenText text, Func<PlacedObject, ObjectDefinition> currentDefinitionOf)
    {
        _font = text.Fonts.FontAt(UiTheme.PromptTextPixels);
        _currentDefinitionOf = currentDefinitionOf;
    }

    /// <summary>Fades toward the target; measures the key and the prompt again only when what's shown or the device changed.</summary>
    /// <param name="target">What the player faces, or null to fade the prompt out.</param>
    /// <param name="device">The device whose key the prompt names.</param>
    /// <param name="delta">The time since the last update.</param>
    public void Update(InteractionTarget? target, InputDevice device, TimeSpan delta)
    {
        if (target is null)
        {
            _fade.Hide();
        }
        else
        {
            _fade.Show(target);
        }

        _fade.Update(delta);
        var shown = _fade.Shown;
        if (ReferenceEquals(shown, _measured) && device == _device)
        {
            return;
        }

        _measured = shown;
        _device = device;
        if (shown is not null)
        {
            Measure(shown, device);
        }
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Vector2 cameraOrigin)
    {
        var opacity = _fade.Opacity;
        if (_measured is null || opacity <= 0)
        {
            return;
        }

        var box = PromptPlacement.BoxFor(_measured.Placed, _currentDefinitionOf(_measured.Placed), cameraOrigin, _boxSize);
        _pill.Draw(batch, textures, box, opacity);
        var keyCap = new Rectangle(box.X + UiTheme.PromptPadding.X, box.Y + UiTheme.PromptPadding.Y, _keyCapWidth, _font.LineHeight);
        _keyCap.Draw(batch, textures, keyCap, opacity);
        batch.DrawString(_font, _key.Text, new Vector2(keyCap.X + KeyCapMargin, keyCap.Y), UiTheme.KeyCapText * opacity);
        batch.DrawString(_font, _prompt.Text, new Vector2(keyCap.Right + KeyGap, keyCap.Y), UiTheme.TextPrimary * opacity);
    }

    private void Measure(InteractionTarget shown, InputDevice device)
    {
        _key = MeasuredText.Of(_font, ButtonLabels.Of(GameButton.Confirm, device));

        // Data refuses an interact event without a prompt, so the empty string never shows.
        _prompt = MeasuredText.Of(_font, shown.Event.Prompt ?? string.Empty);
        _keyCapWidth = (int)Math.Ceiling(_key.Width) + (2 * KeyCapMargin);
        var padding = UiTheme.PromptPadding;
        _boxSize = new Point(
            _keyCapWidth + KeyGap + (int)Math.Ceiling(_prompt.Width) + (2 * padding.X),
            _font.LineHeight + (2 * padding.Y));
    }
}
