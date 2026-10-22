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
/// Above the object the player faces: the device's confirm key, then the event's prompt, such as "E  Open". Plain for
/// now: white text on a flat dark box.
/// </summary>
internal sealed class InteractionPromptView
{
    private const int TextPixels = 22;
    private const int PaddingX = 12;
    private const int PaddingY = 4;
    private const int KeyGap = 14;

    private readonly SpriteFontBase _font;
    private readonly Func<PlacedObject, ObjectDefinition> _currentDefinitionOf;
    private InteractionTarget? _target;
    private InputDevice _device;
    private MeasuredText _key;
    private MeasuredText _prompt;
    private Point _boxSize;

    /// <param name="text">The fonts.</param>
    /// <param name="currentDefinitionOf">What an object is now, after any swap, for the prompt's width over it.</param>
    public InteractionPromptView(ScreenText text, Func<PlacedObject, ObjectDefinition> currentDefinitionOf)
    {
        _font = text.Fonts.FontAt(TextPixels);
        _currentDefinitionOf = currentDefinitionOf;
    }

    /// <summary>Measures the key and the prompt again only when the target or the device changed.</summary>
    public void Update(InteractionTarget? target, InputDevice device)
    {
        if (ReferenceEquals(target, _target) && device == _device)
        {
            return;
        }

        _target = target;
        _device = device;
        if (target is null)
        {
            return;
        }

        _key = MeasuredText.Of(_font, ButtonLabels.Of(GameButton.Confirm, device));

        // Data refuses an interact event without a prompt, so the empty string never shows.
        _prompt = MeasuredText.Of(_font, target.Event.Prompt ?? string.Empty);
        _boxSize = new Point(
            (int)Math.Ceiling(_key.Width + KeyGap + _prompt.Width) + (2 * PaddingX),
            _font.LineHeight + (2 * PaddingY));
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Vector2 cameraOrigin)
    {
        if (_target is null)
        {
            return;
        }

        var box = PromptPlacement.BoxFor(_target.Placed, _currentDefinitionOf(_target.Placed), cameraOrigin, _boxSize);
        batch.Draw(textures.Pixel, box, UiTheme.Scrim);
        var keyPosition = new Vector2(box.X + PaddingX, box.Y + PaddingY);
        batch.DrawString(_font, _key.Text, keyPosition, UiTheme.TextPrimary);
        batch.DrawString(_font, _prompt.Text, keyPosition + new Vector2(_key.Width + KeyGap, 0), UiTheme.TextPrimary);
    }
}
