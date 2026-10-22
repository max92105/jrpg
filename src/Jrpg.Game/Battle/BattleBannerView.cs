using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the banner: centered 190 px from the top, the message on a dark band that fades out at both ends, between two
/// bright hairlines.
/// </summary>
internal sealed class BattleBannerView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56).
    private const int Top = 190;
    private const int PaddingX = 120;
    private const int PaddingY = 10;
    private const int MinWidth = 560;
    private const int TextPixels = 22;

    private readonly BattleBanner _banner;
    private readonly UiTextures _ui;
    private readonly SpriteFontBase _font;
    private MeasuredText _text = new(string.Empty, 0);

    public BattleBannerView(BattleBanner banner, ScreenText text, UiTextures ui)
    {
        _banner = banner;
        _ui = ui;
        _font = text.Fonts.FontAt(TextPixels);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures)
    {
        var opacity = _banner.Opacity;
        if (opacity <= 0)
        {
            return;
        }

        if (!ReferenceEquals(_text.Text, _banner.Text))
        {
            _text = MeasuredText.Of(_font, _banner.Text);
        }

        var width = Math.Max(MinWidth, (int)_text.Width + (2 * PaddingX));
        var band = new Rectangle((VirtualScreen.Width - width) / 2, Top, width, (int)_font.LineHeight + (2 * PaddingY));
        var tint = Color.White * opacity;
        batch.Draw(_ui.Gradient(textures, UiTheme.Banner), band, tint);
        var line = _ui.Gradient(textures, UiTheme.HighlightLine);
        batch.Draw(line, new Rectangle(band.X, band.Y, band.Width, 1), tint);
        batch.Draw(line, new Rectangle(band.X, band.Bottom - 1, band.Width, 1), tint);
        batch.DrawString(_font, _text.Text, new Vector2(band.Center.X - (_text.Width / 2), band.Y + PaddingY), UiTheme.TextPrimary * opacity);
    }
}
