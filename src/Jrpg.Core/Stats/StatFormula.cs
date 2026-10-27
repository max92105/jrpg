namespace Jrpg.Core.Stats;

/// <summary>
/// The primaries-to-derived conversions, in one place (ADR 0026). Each is a percent per point with no level divisor,
/// applied to a base that grows, so a point keeps its worth at every level; each rounds down once, at the end.
/// </summary>
public static class StatFormula
{
    // Rates are percents per 100 points, so a percent per point is rate / 100 and the whole factor is over 100 x 100.
    private const long PercentPerPointScale = 100L * 100;

    /// <summary>
    /// The class's max HP at the member's level raised by Constitution:
    /// <c>classMaxHp × (100 + constitution × rate ÷ 100) ÷ 100</c>. A percent of a base that grows, so Constitution,
    /// from the class or from gear, keeps its worth as the member levels (ADR 0026): 220 HP and 14 Constitution at 100
    /// per 100 points give 250.
    /// </summary>
    public static int MaxHp(int classMaxHp, int constitution, StatRules rules) =>
        (int)(classMaxHp * (PercentPerPointScale + ((long)constitution * rules.HpPercentPer100Constitution)) / PercentPerPointScale);
}
