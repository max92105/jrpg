using Microsoft.Xna.Framework;

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
}
