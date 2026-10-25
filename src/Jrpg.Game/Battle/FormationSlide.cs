using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Battle;

/// <summary>
/// The party easing from its old places to its new ones after a swap (ADR 0023), so two battlers trading places slide
/// instead of jumping: a clock that starts again whenever places change, and the easing between two positions.
/// </summary>
internal sealed class FormationSlide
{
    /// <summary>How long the slide lasts (design.md, SwapSlide).</summary>
    public static readonly TimeSpan Duration = TimeSpan.FromSeconds(0.25);

    private TimeSpan _elapsed = Duration;

    /// <summary>How far the slide has gone, eased: 0 at the old places, 1 at the new ones or when still.</summary>
    private float Share => Easing.CubicOut(Easing.ShareOf(_elapsed, Duration));

    public void Restart() => _elapsed = TimeSpan.Zero;

    public void Update(TimeSpan delta) => _elapsed += delta;

    public Rectangle SpotOf(Rectangle from, Rectangle to) =>
        new(PositionOf(from.X, to.X), PositionOf(from.Y, to.Y), to.Width, to.Height);

    public int PositionOf(int from, int to) => from + (int)MathF.Round((to - from) * Share);
}
