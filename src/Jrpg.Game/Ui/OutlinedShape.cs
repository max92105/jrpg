using Jrpg.Core.Geometry;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// A vector path baked into pixels (such as a menu tab's arrow): filled, with a one-pixel outline, transparent outside.
/// </summary>
internal static class OutlinedShape
{
    /// <param name="corners">The path's corners, in fractions of the size (0 to 1), as <see cref="CanopyMask.Build"/> takes them.</param>
    /// <param name="size">The image's width and height in pixels.</param>
    /// <param name="fill">One color per column, left to right (<see cref="GradientStrip"/>), so the fill can be a gradient.</param>
    /// <param name="edge">The one-pixel outline.</param>
    /// <returns>Premultiplied pixels, row by row.</returns>
    public static Color[] Pixels(IReadOnlyList<Point2D> corners, Point size, IReadOnlyList<Color> fill, Color edge)
    {
        var inside = CanopyMask.Build([corners], size.X, size.Y);
        var pixels = new Color[size.X * size.Y];
        for (var y = 0; y < size.Y; y++)
        {
            for (var x = 0; x < size.X; x++)
            {
                var index = (y * size.X) + x;
                if (inside[index])
                {
                    pixels[index] = IsEdge(inside, size, x, y) ? edge : fill[x];
                }
            }
        }

        return pixels;
    }

    /// <summary>The same shape filled with no outline, as a path drawn without a stroke.</summary>
    /// <param name="corners">The path's corners, in fractions of the size (0 to 1).</param>
    /// <param name="size">The image's width and height in pixels.</param>
    /// <param name="fill">One color per column, left to right.</param>
    /// <returns>Premultiplied pixels, row by row.</returns>
    public static Color[] Filled(IReadOnlyList<Point2D> corners, Point size, IReadOnlyList<Color> fill)
    {
        var inside = CanopyMask.Build([corners], size.X, size.Y);
        var pixels = new Color[inside.Length];
        for (var index = 0; index < inside.Length; index++)
        {
            if (inside[index])
            {
                pixels[index] = fill[index % size.X];
            }
        }

        return pixels;
    }

    // Inside, with a neighbor outside the shape or past the image's border.
    private static bool IsEdge(bool[] inside, Point size, int x, int y) =>
        !IsInside(inside, size, x - 1, y) || !IsInside(inside, size, x + 1, y) || !IsInside(inside, size, x, y - 1) || !IsInside(inside, size, x, y + 1);

    private static bool IsInside(bool[] inside, Point size, int x, int y) =>
        x >= 0 && x < size.X && y >= 0 && y < size.Y && inside[(y * size.X) + x];
}
