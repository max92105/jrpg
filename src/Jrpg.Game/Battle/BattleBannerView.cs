using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the banner as the clip's plaque (decision 646): a small near-black navy plaque hugging the action's name, a
/// hairline along its top and bottom, its top 190 px down, centered across the screen, under the turn order.
/// </summary>
internal sealed class BattleBannerView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56): the plaque's top, the name's size.
    private const int Top = 190;
    private const int TextPixels = 24;
    private const int HairlineHeight = 1;

    private readonly BattleBanner _banner;
    private readonly SpriteFontBase _font;
    private readonly UiTextures _ui;
    private MeasuredText _text;
    private PlaqueLayout _plaque;

    public BattleBannerView(BattleBanner banner, ScreenText text, UiTextures ui)
    {
        _banner = banner;
        _ui = ui;
        _font = text.Fonts.FontAt(TextPixels);
        Measure(string.Empty);
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
            Measure(_banner.Text);
        }

        var plaque = _plaque.Plaque;
        batch.Draw(textures.Pixel, plaque, UiTheme.BannerPlaque * opacity);
        var line = _ui.Gradient(textures, UiTheme.HighlightLine);
        batch.Draw(line, new Rectangle(plaque.X, plaque.Y, plaque.Width, HairlineHeight), Color.White * opacity);
        var bottomLine = new Rectangle(plaque.X, plaque.Bottom - HairlineHeight, plaque.Width, HairlineHeight);
        batch.Draw(line, bottomLine, Color.White * opacity);
        batch.DrawString(_font, _text.Text, _plaque.TextPosition, UiTheme.TextPrimary * opacity);
    }

    // Measured and laid out only when the text changes, never per frame.
    private void Measure(string text)
    {
        _text = MeasuredText.Of(_font, text);
        _plaque = PlaqueLayout.Around(new Vector2(_text.Width, _font.LineHeight), VirtualScreen.Width / 2, Top);
    }
}
