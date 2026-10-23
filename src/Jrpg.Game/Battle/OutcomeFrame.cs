using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Battle;

/// <summary>
/// The frame the victory and defeat screens share: the battle dimmed behind them, the title's place over a hairline,
/// and the footer with a second hairline above it.
/// </summary>
internal static class OutcomeFrame
{
    // Placeholder layout on the 1920 x 1080 virtual screen (decision 56).
    public const int TitleLeft = 104;
    public const int TitleTop = 112;
    public const int FooterHeight = 100;
    private const int TitleLineLeft = 96;
    private const int TitleLineTop = 226;
    private const int FooterLineAbove = 170;

    /// <summary>The hairline above the footer, which the screen's content must end above.</summary>
    public const int FooterLineTop = VirtualScreen.Height - FooterLineAbove;

    /// <summary>The dimmed battle and the title's hairline; the screen draws its title over them.</summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the plain pixel comes from.</param>
    /// <param name="ui">Where the scrim is baked.</param>
    public static void DrawBackdrop(SpriteBatch batch, TextureLibrary textures, UiTextures ui)
    {
        // Decision 52: the battle is dimmed, not blurred.
        batch.Draw(ui.MenuScrim(textures), new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), Color.White);
        batch.Draw(textures.Pixel, new Rectangle(TitleLineLeft, TitleLineTop, VirtualScreen.Width - TitleLineLeft, 1), UiTheme.BarLine);
    }

    /// <summary>The hairline above the footer, then the footer bar; the screen writes its hint on it.</summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel.</param>
    public static void DrawFooter(SpriteBatch batch, Texture2D pixel)
    {
        batch.Draw(pixel, new Rectangle(0, FooterLineTop, VirtualScreen.Width, 1), UiTheme.BarLine);
        FooterBar.Draw(batch, pixel, FooterHeight, opacity: 1);
    }
}
