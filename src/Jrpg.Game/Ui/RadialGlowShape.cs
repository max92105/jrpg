using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// A soft white disc baked into a square texture, stretched to the impact's floor ellipse and its halo (decision 660):
/// opaque at the center, clear at the edge, with no visible rim.
/// </summary>
internal static class RadialGlowShape
{
    /// <summary>The side it is baked at: soft light shows no texels at the sizes it is stretched to.</summary>
    public const int Size = 128;

    /// <returns><see cref="Size"/> x <see cref="Size"/> premultiplied white pixels, row by row.</returns>
    public static Color[] Pixels()
    {
        var pixels = new Color[Size * Size];
        var half = Size / 2f;
        for (var y = 0; y < Size; y++)
        {
            var dy = (y + 0.5f - half) / half;
            for (var x = 0; x < Size; x++)
            {
                var dx = (x + 0.5f - half) / half;
                pixels[(y * Size) + x] = Color.White * StrengthAt((dx * dx) + (dy * dy));
            }
        }

        return pixels;
    }

    // (1 - d²)², d the distance from the center in half-sides: flat at the center, falling to 0 at the edge with no
    // slope there, so the stretched disc has no rim.
    private static float StrengthAt(float distanceSquared)
    {
        var inside = Math.Max(0, 1 - distanceSquared);
        return inside * inside;
    }
}
