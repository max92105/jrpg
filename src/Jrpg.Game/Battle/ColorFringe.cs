using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// How far the impact's bright parts split into red and green as time passes since damage landed (decision 661): 6
/// virtual px at the impact, closing fast then settling, back to one picture by 0.167 s, as in the reference clip's
/// frames #77-#82. Placeholder values, tuned by eye against the clip.
/// </summary>
internal static class ColorFringe
{
    private const float Height = 6;
    private static readonly TimeSpan _length = TimeSpan.FromSeconds(0.167);

    /// <param name="sinceImpact">From the impact on; before it, nothing splits.</param>
    /// <returns>Virtual px between the red copy and the blue one, as between the blue and the green.</returns>
    public static float ShiftAt(TimeSpan sinceImpact) =>
        sinceImpact < TimeSpan.Zero ? 0 : Height * (1 - Easing.CubicOut(Easing.ShareOf(sinceImpact, _length)));

    /// <summary>
    /// <see cref="ShiftAt"/> to the nearest whole pixel, as the copies are drawn: 0 once they would land on the same
    /// place, so nothing is split for nothing.
    /// </summary>
    public static int PixelsAt(TimeSpan sinceImpact) => (int)MathF.Round(ShiftAt(sinceImpact));
}
