using Jrpg.Data.Art;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Draws a cinematic's still (ADR 0046) over black: the picture, its subtitle in a dark band that fades out upward, and
/// the skip hint at the bottom right (<see cref="SkipHintView"/>).
/// </summary>
internal sealed class StillView
{
    // Placeholder look (decision 82): subtitles 32 px with their middle 80 px above the bottom, over a 160 px band.
    private const int CaptionTextPixels = 32;
    private const int CaptionMiddleAbove = 80;
    private const int CaptionBandHeight = 160;

    private static readonly Rectangle _screen = new(0, 0, VirtualScreen.Width, VirtualScreen.Height);
    private static readonly Rectangle _band = new(0, VirtualScreen.Height - CaptionBandHeight, VirtualScreen.Width, CaptionBandHeight);

    private readonly OutlinedFont _captionFont;
    private readonly SkipHintView _hint;
    private Texture2D? _bandTexture;
    private MeasuredText? _caption;

    public StillView(ScreenText text, UiTextures ui)
    {
        _captionFont = new OutlinedFont(text.Fonts.FontAt(CaptionTextPixels));
        _hint = new SkipHintView(text, ui);
    }

    /// <summary>Measures the still's subtitle once, when it starts; null shows none.</summary>
    public void Begin(string? caption) =>
        _caption = caption is null ? null : MeasuredText.Of(_captionFont.Font, caption);

    /// <summary>The skip hint for the device last used.</summary>
    public void Follow(InputDevice device) => _hint.Follow(device);

    /// <summary>The skip hint as one line, as a test reads it.</summary>
    internal string SkipHint => _hint.Text;

    /// <summary>
    /// Draws the still: black, then its picture in a batch of its own with linear sampling (decision 627), so a picture
    /// scaled to the screen, or drifting by fractions of a pixel, stays smooth; then its subtitle and the hint, sharp,
    /// in the scene's batch again.
    /// </summary>
    /// <param name="batch">Begun with <see cref="VirtualScreen.BeginBatch"/>; ended, then begun again with it.</param>
    /// <param name="textures">The still's picture, and where the band is made once.</param>
    /// <param name="frame">The still as it shows this frame.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, StillFrame frame)
    {
        var tint = Color.White * frame.Opacity;
        batch.End();
        batch.Begin(samplerState: SamplerState.LinearClamp);
        batch.Draw(textures.Pixel, _screen, Color.Black);
        CoverFit.DrawZoomedOverScreen(batch, textures.Get(ArtFolders.Cinematics, frame.ImageFile), tint, frame.Zoom);
        batch.End();
        VirtualScreen.BeginBatch(batch);
        if (_caption is { } caption)
        {
            _bandTexture ??= textures.Create(1, CaptionBandHeight, GradientStrip.Colors(UiTheme.CaptionBand, CaptionBandHeight));
            batch.Draw(_bandTexture, _band, tint);
            var position = new Vector2((VirtualScreen.Width - caption.Width) / 2, VirtualScreen.Height - CaptionMiddleAbove - (_captionFont.Font.LineHeight / 2f));
            _captionFont.Draw(batch, caption.Text, position, UiTheme.TextPrimary * frame.Opacity);
        }

        // The hint stays while the stills run, so the player can skip at any time.
        _hint.Draw(batch);
    }

    /// <summary>The hold to skip's ring around the hint's key, over a cinematic's still (decision 626).</summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the ring's segment is made once.</param>
    /// <param name="share">0 to 1: how far the hold has come.</param>
    public void DrawSkipRing(SpriteBatch batch, TextureLibrary textures, float share) => _hint.DrawRing(batch, textures, share);
}
