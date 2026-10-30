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
}
