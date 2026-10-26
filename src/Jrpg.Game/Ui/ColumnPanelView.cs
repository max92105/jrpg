using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws a menu page's column (decision 269): a dark translucent fill from the header to the footer with a hairline at
/// its left, so the content reads over the art. Columns side by side share that line as their divider, so only the last
/// one draws a right edge too.
/// </summary>
internal sealed class ColumnPanelView(ColumnPanelStyle style)
{
    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel.</param>
    /// <param name="column">The column's area, usually at full height (<c>MenuLayout.FullHeight</c>).</param>
    /// <param name="opacity">0 to 1, while the page fades in.</param>
    public void Draw(SpriteBatch batch, Texture2D pixel, Rectangle column, float opacity)
    {
        batch.Draw(pixel, column, style.Fill * opacity);
        batch.Draw(pixel, column with { Width = 1 }, style.Edge * opacity);
    }

    /// <summary>The line down the right of the page's last column, which no column beside it draws.</summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel.</param>
    /// <param name="column">The column's area.</param>
    /// <param name="opacity">0 to 1, while the page fades in.</param>
    public void DrawRightEdge(SpriteBatch batch, Texture2D pixel, Rectangle column, float opacity) =>
        batch.Draw(pixel, column with { X = column.Right - 1, Width = 1 }, style.Edge * opacity);

    /// <summary>A column standing alone: its fill and both its side lines.</summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="pixel">The plain white pixel.</param>
    /// <param name="column">The column's area.</param>
    /// <param name="opacity">0 to 1, while the page fades in.</param>
    public void DrawClosed(SpriteBatch batch, Texture2D pixel, Rectangle column, float opacity)
    {
        Draw(batch, pixel, column, opacity);
        DrawRightEdge(batch, pixel, column, opacity);
    }
}
