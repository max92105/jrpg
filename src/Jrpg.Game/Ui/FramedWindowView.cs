using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws a framed window from its <see cref="FramedWindowLayout"/>: the dark translucent body, the outer hairline, and
/// the inner one as four lines stopping short of the corners, so they stay open like the reference UI's. Nothing is
/// baked; the caller writes its content into the layout's body.
/// </summary>
internal static class FramedWindowView
{
    // The inner hairline's lines stop 6 px short of each corner (placeholder).
    private const int CornerGap = 6;

    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel, tinted.</param>
    /// <param name="layout">Where the window's parts go.</param>
    /// <param name="opacity">0 to 1, the window's fade.</param>
    public static void Draw(SpriteBatch batch, Texture2D pixel, FramedWindowLayout layout, float opacity)
    {
        batch.Draw(pixel, layout.Frame, UiTheme.Surface * opacity);
        var line = UiTheme.WindowLine * opacity;
        Outline.Draw(batch, pixel, layout.Frame, line);
        var inner = layout.InnerFrame;
        var across = inner.Width - (2 * CornerGap);
        var down = inner.Height - (2 * CornerGap);
        batch.Draw(pixel, new Rectangle(inner.X + CornerGap, inner.Y, across, 1), line);
        batch.Draw(pixel, new Rectangle(inner.X + CornerGap, inner.Bottom - 1, across, 1), line);
        batch.Draw(pixel, new Rectangle(inner.X, inner.Y + CornerGap, 1, down), line);
        batch.Draw(pixel, new Rectangle(inner.Right - 1, inner.Y + CornerGap, 1, down), line);
    }
}
