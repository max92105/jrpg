using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws a framed window from its <see cref="FramedWindowLayout"/>: the dark translucent body, the outer hairline, and
/// the inner one as four lines stopping short of the corners, so they stay open like the reference UI's; a titled
/// window also gets its separator, fading at both ends around a small diamond. The caller writes the title and the
/// content.
/// </summary>
/// <param name="ui">Where the separator's gradient is made, once.</param>
internal sealed class FramedWindowView(UiTextures ui)
{
    // The inner hairline's lines stop 6 px short of each corner; the separator's diamond is 10 px across
    // (placeholders).
    private const int CornerGap = 6;
    private const float SeparatorDiamond = 10;

    /// <param name="batch">Already begun.</param>
    /// <param name="textures">Where the shared textures are made.</param>
    /// <param name="layout">Where the window's parts go.</param>
    /// <param name="opacity">0 to 1, the window's fade.</param>
    public void Draw(SpriteBatch batch, TextureLibrary textures, FramedWindowLayout layout, float opacity)
    {
        var pixel = textures.Pixel;
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
        if (layout.HasTitle)
        {
            DrawSeparator(batch, textures, layout, opacity);
        }
    }

    // As wide as the body, so it lines up with the content under it.
    private void DrawSeparator(SpriteBatch batch, TextureLibrary textures, FramedWindowLayout layout, float opacity)
    {
        var body = layout.Body;
        batch.Draw(ui.Gradient(textures, UiTheme.WindowSeparator), new Rectangle(body.X, layout.SeparatorY, body.Width, 1), Color.White * opacity);
        Diamond.WithDiagonal(new Vector2(body.Center.X, layout.SeparatorY + 0.5f), SeparatorDiamond).Draw(batch, textures.Pixel, UiTheme.TextPrimary * opacity);
    }
}
