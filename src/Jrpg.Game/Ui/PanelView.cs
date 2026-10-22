using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Ui;

/// <summary>
/// Draws a rounded panel at any size from one small baked texture: the corners as they are, the middle row and column
/// stretched (a 9-slice), so corners never blur or stretch.
/// </summary>
internal sealed class PanelView(PanelStyle style)
{
    private const int SliceCount = 3;

    private Texture2D? _texture;

    /// <summary>
    /// The texture's pixels, row by row: two corners plus one middle pixel square. Edge pixels are shaded by how much of
    /// them the rounded outline covers, so the corners stay smooth.
    /// </summary>
    public static Color[] BakePixels(PanelStyle style)
    {
        var radius = style.CornerRadius;
        var size = SizeOf(style);
        var pixels = new Color[size * size];
        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var depth = radius - DistanceToMiddle(x, y, radius);
                var coverage = Math.Clamp(depth + 0.5f, 0, 1);
                var borderShare = Math.Clamp(style.BorderWidth - depth + 0.5f, 0, 1);
                pixels[(y * size) + x] = Color.Lerp(style.Fill, style.Border, borderShare) * coverage;
            }
        }

        return pixels;
    }

    /// <summary>
    /// Draws the panel over <c>area</c>, which must be at least two corner radii wide and high, at <c>opacity</c> (0
    /// transparent to 1 opaque).
    /// </summary>
    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
        var tint = Color.White * opacity;
        var size = SizeOf(style);
        _texture ??= textures.Create(size, size, BakePixels(style));
        for (var row = 0; row < SliceCount; row++)
        {
            var (top, height) = DestinationSlice(area.Y, area.Height, row);
            var (sourceTop, sourceHeight) = SourceSlice(row);
            for (var column = 0; column < SliceCount; column++)
            {
                var (left, width) = DestinationSlice(area.X, area.Width, column);
                var (sourceLeft, sourceWidth) = SourceSlice(column);
                batch.Draw(_texture, new Rectangle(left, top, width, height), new Rectangle(sourceLeft, sourceTop, sourceWidth, sourceHeight), tint);
            }
        }
    }

    private static int SizeOf(PanelStyle style) => (2 * style.CornerRadius) + 1;

    // From the pixel's center to the middle pixel's square; the outline is every point one radius away from it, so the
    // edges are straight and the corners round. Inside the panel, radius minus this is how deep the pixel lies.
    private static float DistanceToMiddle(int x, int y, int radius)
    {
        var centerX = x + 0.5f;
        var centerY = y + 0.5f;
        var dx = centerX - Math.Clamp(centerX, radius, radius + 1);
        var dy = centerY - Math.Clamp(centerY, radius, radius + 1);
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    // Slices 0 and 2 are the corners, one radius long; slice 1 is the middle, one texture pixel stretched over the rest.
    private (int Start, int Length) DestinationSlice(int start, int length, int slice) => slice switch
    {
        0 => (start, style.CornerRadius),
        1 => (start + style.CornerRadius, length - (2 * style.CornerRadius)),
        _ => (start + length - style.CornerRadius, style.CornerRadius),
    };

    private (int Start, int Length) SourceSlice(int slice) => slice switch
    {
        0 => (0, style.CornerRadius),
        1 => (style.CornerRadius, 1),
        _ => (style.CornerRadius + 1, style.CornerRadius),
    };
}
