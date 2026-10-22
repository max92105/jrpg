using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// A diamond, or a diamond ring, baked into a square texture, such as the turn order's portrait backdrop and frames.
/// Reaches are fractions of half the square: 1 touches its edges' middles. Edges are smoothed over a pixel.
/// Equal shapes are equal records, so a shape is its own texture cache key.
/// </summary>
/// <param name="Size">The square's side in pixels.</param>
/// <param name="InnerReach">Where the ring starts; 0 fills the diamond.</param>
/// <param name="OuterReach">Where the diamond ends.</param>
/// <param name="Upper">The color above the middle, premultiplied.</param>
/// <param name="Lower">The color from the middle down, premultiplied.</param>
internal sealed record DiamondShape(int Size, float InnerReach, float OuterReach, Color Upper, Color Lower)
{
    /// <returns><see cref="Size"/> x <see cref="Size"/> premultiplied pixels, row by row.</returns>
    public Color[] Pixels()
    {
        var pixels = new Color[Size * Size];
        var half = Size / 2f;
        for (var y = 0; y < Size; y++)
        {
            var dy = MathF.Abs(y + 0.5f - half);
            var color = y + 0.5f < half ? Upper : Lower;
            for (var x = 0; x < Size; x++)
            {
                var reach = (MathF.Abs(x + 0.5f - half) + dy) / half;
                pixels[(y * Size) + x] = color * Coverage(reach, half);
            }
        }

        return pixels;
    }

    // How much of a pixel at this reach lies inside, ramping over one pixel at each edge.
    private float Coverage(float reach, float half)
    {
        var insideOuter = Math.Clamp(((OuterReach - reach) * half) + 0.5f, 0, 1);
        var outsideInner = InnerReach <= 0 ? 1 : Math.Clamp(((reach - InnerReach) * half) + 0.5f, 0, 1);
        return insideOuter * outsideInner;
    }
}
