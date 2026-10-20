using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>A one-pixel border drawn just inside a rectangle, as a WPF <c>Border</c> with thickness 1.</summary>
internal static class Outline
{
    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel, tinted.</param>
    /// <param name="area">The rectangle the border runs inside.</param>
    /// <param name="color">Already faded with the screen.</param>
    public static void Draw(SpriteBatch batch, Texture2D pixel, Rectangle area, Color color)
    {
        batch.Draw(pixel, new Rectangle(area.X, area.Y, area.Width, 1), color);
        batch.Draw(pixel, new Rectangle(area.X, area.Bottom - 1, area.Width, 1), color);
        batch.Draw(pixel, new Rectangle(area.X, area.Y, 1, area.Height), color);
        batch.Draw(pixel, new Rectangle(area.Right - 1, area.Y, 1, area.Height), color);
    }
}
