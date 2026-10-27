namespace Jrpg.Core.Resources;

/// <summary>
/// The points a holder's Will adds to a resource (decision 291: no Energy stat, Will scales each class's own resource
/// instead). Placeholder: one point per <c>willPerPoint</c>, rounded toward zero, so a point of Will is worth the same
/// at every level and a resource grows in steps a player can aim for, like the speed charges.
/// </summary>
public static class WillFormula
{
    /// <param name="will">The holder's Will, gear included; below 0 it takes points away.</param>
    /// <param name="willPerPoint">The Will one point takes, at least 1.</param>
    public static int BonusPoints(int will, int willPerPoint) => will / willPerPoint;
}
