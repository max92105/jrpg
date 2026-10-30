using Jrpg.Core.Items;

namespace Jrpg.Core.Trade;

/// <summary>
/// What items and pieces cost in a shop (ADR 0031): an item its price, a piece its base price plus its affixes' gold
/// values (decision 375). Takes the values as a plain map, so the trade never reads the affix pool.
/// </summary>
public sealed class Pricing
{
    private readonly IReadOnlyDictionary<string, int> _affixValues;

    /// <param name="affixValues">Each affix id to the gold it adds to a piece.</param>
    public Pricing(IReadOnlyDictionary<string, int> affixValues)
    {
        _affixValues = affixValues;
    }

    /// <exception cref="InvalidOperationException">
    /// The item has no price: a content bug, since the loader refuses a shop listing one.
    /// </exception>
    public static int PriceOf(ItemDefinition item) =>
        item.Price ?? throw new InvalidOperationException($"Item '{item.Id}' has no price, so no shop can trade it.");

    public int PriceOf(GearPiece piece) =>
        PriceFormula.PieceValue(PriceOf(piece.Base), piece.Affixes.Select(affix => _affixValues[affix.AffixId]));
}
