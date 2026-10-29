using Jrpg.Core.Items;

namespace Jrpg.Core.Loot;

/// <summary>
/// Drop chances and rarity weights under drop luck (decision 349): luck raises every drop chance and every tier above
/// Common by its percent, so drops and better tiers grow likelier together, and the total grows rather than Common
/// shrinking away. No cap on luck (decision 367): the products are <c>long</c>, and a huge luck holds a tier at
/// <c>int.MaxValue</c>.
/// </summary>
internal static class LootFormula
{
    private const long Whole = 1000;
    private const int PermillePerPercent = 10;

    /// <returns>The chance in permille × (1000 + luck) ÷ 1000, rounded down, at most 1000.</returns>
    /// <param name="chancePercent">A drop entry's chance, in whole percents.</param>
    /// <param name="luckPermille">The party's drop luck.</param>
    public static int ChancePermille(int chancePercent, int luckPermille) =>
        (int)Math.Min(Whole, Raised(chancePercent * PermillePerPercent, luckPermille));

    /// <returns>Common's weight as it is; another tier's × (1000 + luck) ÷ 1000, rounded down.</returns>
    /// <param name="weight">The tier's weight in the rarity table.</param>
    /// <param name="rarity">The tier.</param>
    /// <param name="luckPermille">The party's drop luck.</param>
    public static int RarityWeight(int weight, Rarity rarity, int luckPermille) =>
        rarity == Rarity.Common ? weight : (int)Math.Min(int.MaxValue, Raised(weight, luckPermille));

    private static long Raised(int value, int luckPermille) => value * (Whole + luckPermille) / Whole;
}
