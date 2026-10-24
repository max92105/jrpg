using FontStashSharp;
using Jrpg.Core.Events;
using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Draws a cutscene still (ADR 0016) over black: the picture, its subtitle in a dark band that fades out upward, and a
/// small skip hint at the bottom right naming the device's Confirm button (decisions 171, 172).
/// </summary>
internal sealed class StillView
{
    // Placeholder look (decision 82): subtitles 32 px with their middle 80 px above the bottom, over a 160 px band.
    private const int CaptionTextPixels = 32;
    private const int CaptionMiddleAbove = 80;
    private const int CaptionBandHeight = 160;
    private const int HintTextPixels = 20;
    private const int HintMargin = 32;

    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);
    private static readonly Rectangle _band = new(0, VirtualScreen.Height - CaptionBandHeight, VirtualScreen.Width, CaptionBandHeight);

    private readonly OutlinedFont _captionFont;
    private readonly SpriteFontBase _hintFont;
    private readonly UiMessages _messages;
    private Texture2D? _bandTexture;
    private MeasuredText? _caption;
    private MeasuredText _hint;
    private InputDevice? _hintDevice;

    public StillView(ScreenText text)
    {
        _captionFont = new OutlinedFont(text.Fonts.FontAt(CaptionTextPixels));
        _hintFont = text.Fonts.FontAt(HintTextPixels);
        _messages = text.Messages;
        _hint = MeasuredText.Of(_hintFont, string.Empty);
    }

    /// <summary>Measures the still's subtitle once, when it starts.</summary>
    public void Begin(StillStep still) =>
        _caption = still.Caption is { } caption ? MeasuredText.Of(_captionFont.Font, caption) : null;

    /// <summary>Names the button of the device last used; measured again only when the device changes.</summary>
    public void Follow(InputDevice device)
    {
        if (device == _hintDevice)
        {
            return;
        }

        _hintDevice = device;
        var button = new MessageArgument("button", ButtonLabels.Of(GameButton.Confirm, device));
        _hint = MeasuredText.Of(_hintFont, _messages.Format(MessageIds.CinematicSkip, button));
    }

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">The still's picture, and where the band is made once.</param>
    /// <param name="still">The still on screen; the picture and its subtitle fade with it.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, StillPlayback still)
    {
        var tint = Color.White * still.Opacity;
        batch.Draw(textures.Pixel, _screen, Color.Black);
        CoverFit.DrawOverScreen(batch, textures.Get(ArtFolders.Cinematics, still.Step.ImageFile), tint);
        if (_caption is { } caption)
        {
            _bandTexture ??= textures.Create(1, CaptionBandHeight, GradientStrip.Colors(UiTheme.CaptionBand, CaptionBandHeight));
            batch.Draw(_bandTexture, _band, tint);
            var position = new Vector2((VirtualScreen.Width - caption.Width) / 2, VirtualScreen.Height - CaptionMiddleAbove - (_captionFont.Font.LineHeight / 2f));
            _captionFont.Draw(batch, caption.Text, position, UiTheme.TextPrimary * still.Opacity);
        }

        // The hint stays while the stills run, so the player can skip at any time.
        var hintPosition = new Vector2(VirtualScreen.Width - HintMargin - _hint.Width, VirtualScreen.Height - HintMargin - _hintFont.LineHeight);
        batch.DrawString(_hintFont, _hint.Text, hintPosition, UiTheme.TextSecondary);
    }
}
