using Jrpg.Core.Geometry;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The command panel's header tab, pointed at both ends (legacy <c>Shape.BattleHeader</c>,
/// <c>M16,0 H284 L300,16 L284,32 H16 L0,16 Z</c>) as pixels: filled, with a one-pixel outline, transparent outside.
/// </summary>
internal static class CommandHeaderShape
{
    public const int Width = 300;
    public const int Height = 32;

    private const double ShoulderX = 16.0 / Width;

    private static readonly IReadOnlyList<Point2D> _outline =
        [new(ShoulderX, 0), new(1 - ShoulderX, 0), new(1, 0.5), new(1 - ShoulderX, 1), new(ShoulderX, 1), new(0, 0.5)];

    /// <param name="fill">One color per column, left to right, so the fill can be a gradient.</param>
    /// <param name="outline">The one-pixel edge.</param>
    /// <returns><see cref="Width"/> x <see cref="Height"/> premultiplied pixels, row by row.</returns>
    public static Color[] Pixels(IReadOnlyList<Color> fill, Color outline) =>
        OutlinedShape.Pixels(_outline, new Point(Width, Height), fill, outline);
}
