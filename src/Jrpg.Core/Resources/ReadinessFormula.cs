namespace Jrpg.Core.Resources;

/// <summary>
/// How many charges a speed-scaled resource holds in a battle. Decision 202: the rogue readies "X times a battle
/// depending on speed"; the placeholder is one charge per <c>speedPerCharge</c>, rounded down, at least 1
/// (question 213).
/// </summary>
public static class ReadinessFormula
{
    /// <param name="speed">The holder's speed.</param>
    /// <param name="speedPerCharge">The speed one charge takes, at least 1.</param>
    public static int Charges(int speed, int speedPerCharge) => Math.Max(1, speed / speedPerCharge);
}
