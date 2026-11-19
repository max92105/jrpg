using FontStashSharp;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// The small skip hint at the right of a cinematic's bottom black bar, white so it stands out on it, naming the
/// device's Confirm button (decisions 171, 172): "Hold {button} to skip", the key's name inside a ring that fills as
/// Confirm is held (decisions 623, 626). Measured once per device, drawn without allocating.
/// </summary>
internal sealed class SkipHintView
{
    // Placeholder look (decision 82): 20 px text, 32 px in from the screen's right, its middle the bottom bar's.
    private const int TextPixels = 20;
    private const int Margin = 32;

    // Room between the ring and the hint's words on either side of it (placeholder).
    private const int RingPadding = 4;

    // Whiter than the theme's text colors: the hint must stand out on the black bar.
    private static readonly Color _color = Color.White;

    private readonly SpriteFontBase _font;
    private readonly UiMessages _messages;
    private readonly ProgressRingView _ring;
    private InputDevice? _device;
    private MeasuredText _before;
    private MeasuredText _key;
    private MeasuredText _after;
    private float _keyGap;
    private Vector2 _position;
    private Vector2 _keyCenter;

    public SkipHintView(ScreenText text, UiTextures ui)
    {
        _font = text.Fonts.FontAt(TextPixels);
        _messages = text.Messages;
        _ring = new ProgressRingView(ui);
        _before = _key = _after = MeasuredText.Of(_font, string.Empty);
    }

    /// <summary>Names the button of the device last used; measured again only when the device changes.</summary>
    /// <param name="device">The device last used, whose Confirm the hint names.</param>
    public void Follow(InputDevice device)
    {
        if (device == _device)
        {
            return;
        }

        _device = device;
        var label = ButtonLabels.Of(GameButton.Confirm, device);
        var hint = _messages.Format(MessageIds.CinematicHoldSkip, new MessageArgument("button", label));
        var keyAt = hint.IndexOf(label, StringComparison.Ordinal);
        _before = MeasuredText.Of(_font, keyAt < 0 ? hint : hint[..keyAt]);
        _key = MeasuredText.Of(_font, keyAt < 0 ? string.Empty : label);
        _after = MeasuredText.Of(_font, keyAt < 0 ? string.Empty : hint[(keyAt + label.Length)..]);

        // Room for the ring around the key, whatever the key's name's width.
        _keyGap = Math.Max(0, UiTheme.SkipRingRadius + RingPadding - (_key.Width / 2));
        var width = _before.Width + _keyGap + _key.Width + _keyGap + _after.Width;
        var middle = VirtualScreen.Height - (UiTheme.StillBarHeight / 2f);
        _position = new Vector2(VirtualScreen.Width - Margin - width, middle - (_font.LineHeight / 2f));
        _keyCenter = new Vector2(_position.X + _before.Width + _keyGap + (_key.Width / 2), _position.Y + (_font.LineHeight / 2f));
    }

    /// <summary>The hint as one line, as a test reads it; built on each read, so never per frame.</summary>
    internal string Text => $"{_before.Text}{_key.Text}{_after.Text}";

    /// <param name="batch">Already begun.</param>
    public void Draw(SpriteBatch batch)
    {
        var x = _position.X;
        batch.DrawString(_font, _before.Text, new Vector2(x, _position.Y), _color);
        x += _before.Width + _keyGap;
        batch.DrawString(_font, _key.Text, new Vector2(x, _position.Y), _color);
        x += _key.Width + _keyGap;
        batch.DrawString(_font, _after.Text, new Vector2(x, _position.Y), _color);
    }

    /// <summary>The ring around the key's name, lit up to <paramref name="share"/>; for the hold's hint.</summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the ring's segment is made once.</param>
    /// <param name="share">0 to 1: how far the hold has come.</param>
    public void DrawRing(SpriteBatch batch, TextureLibrary textures, float share) =>
        _ring.Draw(batch, textures, _keyCenter, share);
}
