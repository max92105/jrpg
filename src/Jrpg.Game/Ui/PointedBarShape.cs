using Jrpg.Core.Geometry;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Ui;

/// <summary>
/// The victory screen's pointed bars as pixels, transparent outside: a reward row pointed on the left (legacy
/// <c>Shape.RewardBar</c>, <c>M0,32 L30,0 H800 V64 H30 Z</c>) and a card's name tab pointed at both ends (legacy
/// <c>Shape.CardHeader</c>, <c>M16,0 H364 L380,21 L364,42 H16 L0,21 Z</c>).
/// </summary>
internal static class PointedBarShape
{
    public const int RewardBarWidth = 800;
    public const int RewardBarHeight = 64;
    public const int CardHeaderWidth = 380;
    public const int CardHeaderHeight = 42;

    private const double RewardPointX = 30.0 / RewardBarWidth;
    private const double CardShoulderX = 16.0 / CardHeaderWidth;

    private static readonly IReadOnlyList<Point2D> _rewardBar =
        [new(0, 0.5), new(RewardPointX, 0), new(1, 0), new(1, 1), new(RewardPointX, 1)];

    private static readonly IReadOnlyList<Point2D> _cardHeader =
        [new(CardShoulderX, 0), new(1 - CardShoulderX, 0), new(1, 0.5), new(1 - CardShoulderX, 1), new(CardShoulderX, 1), new(0, 0.5)];

    /// <param name="fill">One color per column, left to right, so the fill can be a gradient; legacy draws no outline.</param>
    /// <returns><see cref="RewardBarWidth"/> x <see cref="RewardBarHeight"/> premultiplied pixels, row by row.</returns>
    public static Color[] RewardBar(IReadOnlyList<Color> fill) =>
        OutlinedShape.Filled(_rewardBar, new Point(RewardBarWidth, RewardBarHeight), fill);

    /// <param name="fill">One color per column, left to right, so the fill can be a gradient.</param>
    /// <param name="outline">The one-pixel edge.</param>
    /// <returns><see cref="CardHeaderWidth"/> x <see cref="CardHeaderHeight"/> premultiplied pixels, row by row.</returns>
    public static Color[] CardHeader(IReadOnlyList<Color> fill, Color outline) =>
        OutlinedShape.Pixels(_cardHeader, new Point(CardHeaderWidth, CardHeaderHeight), fill, outline);
}
