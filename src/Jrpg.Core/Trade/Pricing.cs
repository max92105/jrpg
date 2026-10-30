using Jrpg.Core.Items;

namespace Jrpg.Core.Trade;

/// <summary>
/// What items and pieces cost in a shop (ADR 0031): an item its price, a piece its base price plus its affixes' gold
/// values (decision 375); a sale pays the rules' share of either. Takes the values as a plain map, so the trade never
/// reads the affix pool.
/// </summary>
public sealed class Pricing
{
    private readonly IReadOnlyDictionary<string, int> _affixValues;
    private readonly TradeRules _rules;

    /// <param name="affixValues">Each affix id to the gold it adds to a piece.</param>
    /// <param name="rules">The sell share.</param>
    public Pricing(IReadOnlyDictionary<string, int> affixValues, TradeRules rules)
    {
        _affixValues = affixValues;
        _rules = rules;
    }

    /// <exception cref="InvalidOperationException">
    /// The item has no price: a content bug, since the loader refuses a shop listing one.
    /// </exception>
    public static int PriceOf(ItemDefinition item) =>
        item.Price ?? throw new InvalidOperationException($"Item '{item.Id}' has no price, so no shop can trade it.");

    public int PriceOf(GearPiece piece) =>
        PriceFormula.PieceValue(PriceOf(piece.Base), piece.Affixes.Select(affix => _affixValues[affix.AffixId]));

    /// <summary>What a sale of the item pays; 0 when it has no price, so it is never sold (decision 378).</summary>
    public int SalePriceOf(ItemDefinition item) => item.Price is { } price ? PriceFormula.SalePrice(price, _rules.SellPercent) : 0;

    /// <summary>What a sale of the piece pays: the share of its whole price; 0 when its base has no price.</summary>
    public int SalePriceOf(GearPiece piece) => piece.Base.Price is null ? 0 : PriceFormula.SalePrice(PriceOf(piece), _rules.SellPercent);
}
