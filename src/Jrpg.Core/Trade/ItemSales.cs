using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Core.Trade;

/// <summary>
/// Selling owned items for gold (ADR 0031): a quantity of a stack, or one owned piece (worn ones aren't owned, decision
/// 369), at the sell share of its price. Selling is the same at every shop, so the session holds one. Each sale is
/// quoted first; a thing not for sale (no price, or a share worth nothing) or not owned changes nothing. Gold past the
/// purse's cap is lost, never refused.
/// </summary>
public sealed class ItemSales
{
    private readonly Inventory _inventory;
    private readonly Purse _purse;
    private readonly Pricing _pricing;

    /// <param name="inventory">The party's: where sold copies and pieces leave from.</param>
    /// <param name="purse">The party's gold.</param>
    /// <param name="pricing">What each thing sells for.</param>
    internal ItemSales(Inventory inventory, Purse purse, Pricing pricing)
    {
        _inventory = inventory;
        _purse = purse;
        _pricing = pricing;
    }

    /// <summary>What selling <paramref name="quantity"/> copies would give, and why it can't be done.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="quantity"/> is below 1.</exception>
    public TradeQuote QuoteSale(ItemDefinition item, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        var each = _pricing.SalePriceOf(item);
        return new TradeQuote(PriceFormula.Total(each, quantity), each == 0 ? TradeRefusal.NotForSale : null);
    }

    /// <summary>What selling the piece would give, and why it can't be done.</summary>
    public TradeQuote QuoteSale(GearPiece piece)
    {
        var price = _pricing.SalePriceOf(piece);
        return new TradeQuote(price, price == 0 ? TradeRefusal.NotForSale : null);
    }

    /// <summary>
    /// How many copies can be sold (the picker's limit, decision 377): what is owned, or 0 when not for sale.
    /// </summary>
    public int MostSellable(ItemDefinition item) => _pricing.SalePriceOf(item) == 0 ? 0 : _inventory.QuantityOf(item.Id);

    /// <summary>Takes the copies and adds what they give.</summary>
    /// <returns>False when the item isn't for sale or that many aren't owned; nothing changes then.</returns>
    public bool TrySell(ItemDefinition item, int quantity)
    {
        var quote = QuoteSale(item, quantity);
        if (quote.IsRefused || !_inventory.TryRemove(item.Id, quantity))
        {
            return false;
        }

        _purse.Add(quote.Gold);
        return true;
    }

    /// <summary>Takes the first owned piece equal to <paramref name="piece"/> and adds what it gives.</summary>
    /// <returns>False when the piece isn't for sale or isn't owned; nothing changes then.</returns>
    public bool TrySell(GearPiece piece)
    {
        var quote = QuoteSale(piece);
        if (quote.IsRefused || !_inventory.TryRemoveGear(piece))
        {
            return false;
        }

        _purse.Add(quote.Gold);
        return true;
    }
}
