namespace Jrpg.Core.Resources;

/// <summary>
/// The points a holder's Will adds to a resource (decision 291: no Energy stat, Will scales each class's own resource
/// instead), and what they do to its max and its gains (decision 319). Placeholder: one point per <c>willPerPoint</c>,
/// rounded toward zero, so a point of Will is worth the same at every level and a resource grows in steps a player can
/// aim for, like the speed charges.
/// </summary>
public static class WillFormula
{
    // A resource keeps at least one point of room, so Will taken away by gear never leaves nothing to fill.
    private const int LeastMax = 1;

    // A lowered gain still builds something, so a skill that feeds the resource never does nothing.
    private const int LeastGain = 1;

    /// <param name="will">The holder's Will, gear included; below 0 it takes points away.</param>
    /// <param name="willPerPoint">The Will one point takes, at least 1.</param>
    public static int BonusPoints(int will, int willPerPoint) => will / willPerPoint;

    /// <summary>
    /// A resource's max under its holder's Will: <paramref name="baseMax"/> raised by the points when the rule raises
    /// the max, at least 1; the base as it is otherwise.
    /// </summary>
    /// <param name="baseMax">The definition's max, or the speed charges.</param>
    /// <param name="scaling">The resource's Will rule; null when Will leaves it alone.</param>
    /// <param name="will">The holder's Will, gear included.</param>
    public static int MaxUnder(int baseMax, WillScaling? scaling, int will) =>
        scaling is { Effect: WillEffect.RaiseMax }
            ? Math.Max(LeastMax, baseMax + BonusPoints(will, scaling.WillPerPoint))
            : baseMax;

    /// <summary>
    /// A change to a resource with <paramref name="lowering"/> points taken off each gain: a gain keeps at least 1; a
    /// drain passes as it is.
    /// </summary>
    public static int LoweredGain(int change, int lowering) => change > 0 ? Math.Max(LeastGain, change - lowering) : change;
}
