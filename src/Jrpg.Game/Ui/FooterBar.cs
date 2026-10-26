using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>The bar along the bottom of the screen, under a hairline, that a screen writes its one-line hint on.</summary>
internal static class FooterBar
{
    /// <summary>The bar's place on the virtual screen.</summary>
    /// <param name="height">The bar's height in virtual pixels; each screen picks its own.</param>
    public static Rectangle Area(int height) => new(0, VirtualScreen.Height - height, VirtualScreen.Width, height);

    /// <summary>Where a line of text sits so it is centered in the bar, on a whole pixel so it stays crisp.</summary>
    /// <param name="height">The bar's height in virtual pixels.</param>
    /// <param name="lineHeight">The hint font's line height.</param>
    public static float TextTop(int height, float lineHeight) => VirtualScreen.Height - height + MathF.Floor((height - lineHeight) / 2);

    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel.</param>
    /// <param name="height">The bar's height in virtual pixels.</param>
    /// <param name="opacity">0 to 1.</param>
    public static void Draw(SpriteBatch batch, Texture2D pixel, int height, float opacity)
    {
        var area = Area(height);
        batch.Draw(pixel, area, UiTheme.Bar * opacity);
        batch.Draw(pixel, area with { Height = 1 }, UiTheme.BarLine * opacity);
    }
}
