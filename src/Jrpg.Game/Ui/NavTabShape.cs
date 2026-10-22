using Jrpg.Core.Geometry;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The menu tab's arrow (the path <c>M0,0 H378 L410,28 L378,56 H0 Z</c>) as pixels: filled, with a
/// one-pixel outline, transparent outside.
/// </summary>
internal static class NavTabShape
{
    public const int Width = 410;
    public const int Height = 56;

    /// <summary>Where the arrow's point starts, from the left; the selected tab's bright line stops there.</summary>
    public const int ShoulderPixels = 378;

    private const double ShoulderX = (double)ShoulderPixels / Width;

    private static readonly IReadOnlyList<Point2D> _outline = [new(0, 0), new(ShoulderX, 0), new(1, 0.5), new(ShoulderX, 1), new(0, 1)];

    /// <param name="fill">One color per column, left to right (<see cref="GradientStrip"/>), so the fill can be a gradient.</param>
    /// <param name="outline">The one-pixel edge.</param>
    /// <returns><see cref="Width"/> x <see cref="Height"/> premultiplied pixels, row by row.</returns>
    public static Color[] Pixels(IReadOnlyList<Color> fill, Color outline) =>
        OutlinedShape.Pixels(_outline, new Point(Width, Height), fill, outline);
}
