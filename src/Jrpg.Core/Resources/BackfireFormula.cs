namespace Jrpg.Core.Resources;

/// <summary>
/// The warlock's risk (decision 194: more instability unlocks stronger magic "but becomes increasingly dangerous"): the
/// chance grows by a set share per point held, and a backfire costs a set share of the caster's max HP (question 216).
/// </summary>
public static class BackfireFormula
{
    /// <returns>The chance of a backfire in percent, capped at 100.</returns>
    public static int ChancePercent(int amount, BackfireRule rule) => Math.Min(100, amount * rule.ChancePerPointPercent);

    /// <returns>The HP a backfire costs, rounded down but at least 1, so a backfire always hurts.</returns>
    public static int Damage(int maxHp, BackfireRule rule) => Math.Max(1, maxHp * rule.DamagePercentOfMaxHp / 100);
}
