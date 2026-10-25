namespace Jrpg.Core.Resources;

/// <summary>
/// A skill's power when the force it feeds dominates (decision 195): raised by the rule's percent, rounded down like
/// every other percent in battle so a bonus never rounds a power up past what content says.
/// </summary>
public static class DominanceFormula
{
    public static int Empowered(int power, int bonusPercent) => power * (100 + bonusPercent) / 100;
}
