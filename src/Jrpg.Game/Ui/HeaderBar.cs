using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>The bar along the top of the screen, over a hairline, under which a screen writes its title.</summary>
internal static class HeaderBar
{
    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel.</param>
    /// <param name="height">The bar's height in virtual pixels; each screen picks its own.</param>
    /// <param name="opacity">0 to 1.</param>
    public static void Draw(SpriteBatch batch, Texture2D pixel, int height, float opacity)
    {
        batch.Draw(pixel, new Rectangle(0, 0, VirtualScreen.Width, height), UiTheme.Bar * opacity);
        batch.Draw(pixel, new Rectangle(0, height - 1, VirtualScreen.Width, 1), UiTheme.BarLine * opacity);
    }
}
