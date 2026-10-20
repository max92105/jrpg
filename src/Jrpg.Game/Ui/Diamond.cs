using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// A filled square turned 45° (the reference UI's diamonds: markers, portrait frames, glows), drawn from the plain pixel.
/// </summary>
/// <param name="Center">Where the diamond's middle is.</param>
/// <param name="Side">The turned square's side; its diagonal, tip to tip, is the side times √2.</param>
internal readonly record struct Diamond(Vector2 Center, float Side)
{
    /// <summary>The diamond whose tips are <paramref name="diagonal"/> apart, such as one framing a diamond portrait.</summary>
    public static Diamond WithDiagonal(Vector2 center, float diagonal) => new(center, diagonal / MathF.Sqrt(2));

    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel, tinted.</param>
    /// <param name="color">Already faded with the screen.</param>
    public void Draw(SpriteBatch batch, Texture2D pixel, Color color) =>
        batch.Draw(pixel, Center, null, color, MathHelper.PiOver4, new Vector2(0.5f), new Vector2(Side), SpriteEffects.None, 0);
}
