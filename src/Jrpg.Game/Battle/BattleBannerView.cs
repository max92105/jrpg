using FontStashSharp;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// Draws the banner: the action's name in a small framed window (<see cref="FramedWindowView"/>), its top 190 px down,
/// centered across the screen, under the turn order.
/// </summary>
internal sealed class BattleBannerView
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56): the window's top, its narrowest body.
    private const int Top = 190;
    private const int MinBodyWidth = 300 - (2 * FramedWindowLayout.PaddingX);
    private const int TextPixels = 28;

    private readonly BattleBanner _banner;
    private readonly SpriteFontBase _font;
    private readonly FramedWindowView _windowView;
    private MeasuredText _text;
    private FramedWindowLayout _window;

    public BattleBannerView(BattleBanner banner, ScreenText text, UiTextures ui)
    {
        _banner = banner;
        _windowView = new FramedWindowView(ui);
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

        _windowView.Draw(batch, textures, _window, opacity);
        var body = _window.Body;
        batch.DrawString(_font, _text.Text, new Vector2(body.Center.X - (_text.Width / 2), body.Y), UiTheme.TextPrimary * opacity);
    }

    // Measured and laid out only when the text changes, never per frame.
    private void Measure(string text)
    {
        _text = MeasuredText.Of(_font, text);
        var body = new Point(Math.Max(MinBodyWidth, (int)Math.Ceiling(_text.Width)), (int)_font.LineHeight);
        var frameHeight = body.Y + (2 * FramedWindowLayout.PaddingY);
        _window = FramedWindowLayout.Untitled(new Point(VirtualScreen.Width / 2, Top + (frameHeight / 2)), body);
    }
}
