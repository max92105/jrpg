using Jrpg.Core.Geometry;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The menu tab's arrow (legacy <c>Shape.NavTab</c>, <c>M0,0 H378 L410,28 L378,56 H0 Z</c>) as pixels: filled, with a
/// one-pixel outline, transparent outside.
/// </summary>
internal static class NavTabShape
{
    public const int Width = 410;
    public const int Height = 56;

    /// <summary>Where the arrow's point starts, from the left; the selected tab's bright line stops there.</summary>
    public const int ShoulderPixels = 378;

    private const double ShoulderX = (double)ShoulderPixels / Width;

    private static readonly IReadOnlyList<IReadOnlyList<Point2D>> _outline =
    [
        [new(0, 0), new(ShoulderX, 0), new(1, 0.5), new(ShoulderX, 1), new(0, 1)],
    ];

    /// <param name="fill">One color per column, left to right (<see cref="GradientStrip"/>), so the fill can be a gradient.</param>
    /// <param name="outline">The one-pixel edge.</param>
    /// <returns><see cref="Width"/> x <see cref="Height"/> premultiplied pixels, row by row.</returns>
    public static Color[] Pixels(IReadOnlyList<Color> fill, Color outline)
    {
        var inside = CanopyMask.Build(_outline, Width, Height);
        var pixels = new Color[Width * Height];
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var index = (y * Width) + x;
                if (inside[index])
                {
                    pixels[index] = IsEdge(inside, x, y) ? outline : fill[x];
                }
            }
        }

        return pixels;
    }

    // Inside, with a neighbor outside the shape or past the image's border.
    private static bool IsEdge(bool[] inside, int x, int y) =>
        !IsInside(inside, x - 1, y) || !IsInside(inside, x + 1, y) || !IsInside(inside, x, y - 1) || !IsInside(inside, x, y + 1);

    private static bool IsInside(bool[] inside, int x, int y) =>
        x >= 0 && x < Width && y >= 0 && y < Height && inside[(y * Width) + x];
}
