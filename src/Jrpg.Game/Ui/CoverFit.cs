using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Fills an area with an image without stretching it, like WPF's <c>UniformToFill</c>: the image's center part with the
/// area's proportions, the rest cropped.
/// </summary>
internal static class CoverFit
{
    /// <summary>The centered part of the image that, scaled up or down, exactly covers the area.</summary>
    /// <param name="image">The image's width and height in pixels.</param>
    /// <param name="area">The covered area's width and height.</param>
    public static Rectangle SourceIn(Point image, Point area)
    {
        // Compared by cross-multiplying, so equal proportions crop nothing without floating-point error.
        var isImageWider = (long)image.X * area.Y > (long)image.Y * area.X;
        var width = isImageWider ? (int)Math.Round((double)image.Y * area.X / area.Y) : image.X;
        var height = isImageWider ? image.Y : (int)Math.Round((double)image.X * area.Y / area.X);
        return new Rectangle((image.X - width) / 2, (image.Y - height) / 2, width, height);
    }

    /// <summary>Draws the image covering the whole virtual screen.</summary>
    /// <param name="batch">Already begun.</param>
    /// <param name="image">The picture.</param>
    /// <param name="tint">White, faded to fade the picture.</param>
    public static void DrawOverScreen(SpriteBatch batch, Texture2D image, Color tint)
    {
        var source = SourceIn(new Point(image.Width, image.Height), new Point(VirtualScreen.Width, VirtualScreen.Height));
        batch.Draw(image, new Rectangle(0, 0, VirtualScreen.Width, VirtualScreen.Height), source, tint);
    }

    /// <summary>
    /// Draws the image covering the whole virtual screen at a scale of that fit, about the zoom's origin, with a float
    /// size so a slow zoom moves by fractions of a pixel (decision 617).
    /// </summary>
    /// <param name="batch">
    /// Already begun, best with linear sampling: a scale between whole pixels shimmers point-sampled.
    /// </param>
    /// <param name="image">The picture.</param>
    /// <param name="tint">White, faded to fade the picture.</param>
    /// <param name="zoom">
    /// A scale of 1 covers the screen exactly; above 1 about an origin on the screen still covers it (decision 748).
    /// </param>
    public static void DrawZoomedOverScreen(SpriteBatch batch, Texture2D image, Color tint, ScreenZoom zoom)
    {
        var source = SourceIn(new Point(image.Width, image.Height), new Point(VirtualScreen.Width, VirtualScreen.Height));
        var fit = new Vector2((float)VirtualScreen.Width / source.Width, (float)VirtualScreen.Height / source.Height);

        // The crop's top-left goes where the zoom puts the screen's top-left corner.
        batch.Draw(image, zoom.PositionOf(Vector2.Zero), source, tint, 0, Vector2.Zero, fit * zoom.Scale, SpriteEffects.None, 0);
    }
}
