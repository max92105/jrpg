namespace Jrpg.Core.Resources;

/// <summary>
/// Whether one hit is heavy: it took at least a set share of the target's max HP. Decision 192 gives the paladin
/// conviction on a heavy hit, and question 217 costs the cleric faith on one; the share is content (question 212).
/// </summary>
public static class HeavyHitRule
{
    /// <param name="hpLost">The HP the hit actually took.</param>
    /// <param name="maxHp">The target's max HP.</param>
    /// <param name="thresholdPercent">The share of max HP that makes a hit heavy, 1 to 100.</param>
    /// <returns>Whether <c>hpLost / maxHp ≥ thresholdPercent / 100</c>, in integers so nothing rounds.</returns>
    public static bool IsHeavy(int hpLost, int maxHp, int thresholdPercent) => hpLost * 100L >= maxHp * (long)thresholdPercent;
}
