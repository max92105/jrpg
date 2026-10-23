using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// The menu's "you are here" mark: a small white diamond just left of the line that has the cursor, like the defeat
/// menu's pointer.
/// </summary>
internal static class MenuPointer
{
    // Placeholder size: a 14 px diamond whose right corner stops 4 px short of the line.
    private const float Diagonal = 14;
    private const float Gap = 4;

    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain pixel the diamond is drawn with.</param>
    /// <param name="lineStart">The left end of the pointed line, at its vertical middle.</param>
    /// <param name="opacity">The screen's fade.</param>
    public static void Draw(SpriteBatch batch, Texture2D pixel, Vector2 lineStart, float opacity)
    {
        var center = new Vector2(lineStart.X - Gap - (Diagonal / 2), lineStart.Y);
        Diamond.WithDiagonal(center, Diagonal).Draw(batch, pixel, Color.White * opacity);
    }
}
