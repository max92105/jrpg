using Jrpg.Core.Party;

namespace Jrpg.Core.Trade;

/// <summary>The trade's numbers in one place (ADR 0031).</summary>
public static class PriceFormula
{
    /// <summary>
    /// A rolled piece is worth its base item's price plus what each affix adds (decision 375), summed in a
    /// <see langword="long"/> and capped at <see cref="Purse.MaxGold"/>, which no purse could pay past anyway.
    /// </summary>
    public static int PieceValue(int basePrice, IEnumerable<int> affixValues)
    {
        long total = basePrice;
        foreach (var value in affixValues)
        {
            total += value;
        }

        return (int)Math.Min(total, Purse.MaxGold);
    }

    /// <summary>
    /// What a sale pays: the sell share of the price, rounded down once in a <see langword="long"/>; the shop keeps the
    /// rest (the classic JRPG half, decision 375).
    /// </summary>
    public static int SalePrice(int price, int sellPercent) => (int)((long)price * sellPercent / 100);

    /// <summary>
    /// A quantity's total, in a <see langword="long"/> and capped at <see cref="Purse.MaxGold"/>, so no count can wrap
    /// it.
    /// </summary>
    public static int Total(int each, int quantity) => (int)Math.Min((long)each * quantity, Purse.MaxGold);
}
