using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Core.Trade;

/// <summary>
/// Buying from one shop's stock (ADR 0031). Each purchase is quoted first; a refused one, or a thing the stock doesn't
/// hold, changes nothing. For a stack the room is checked before the gold, so a full bag is named before an empty
/// purse; a piece never fills a stack, so only the gold can refuse it.
/// </summary>
public sealed class Shop
{
    private readonly Inventory _inventory;
    private readonly Purse _purse;
    private readonly Pricing _pricing;

    /// <param name="stock">What the shop sells.</param>
    /// <param name="inventory">The party's: where bought copies and pieces go.</param>
    /// <param name="purse">The party's gold.</param>
    /// <param name="pricing">What each thing costs.</param>
    internal Shop(ShopStock stock, Inventory inventory, Purse purse, Pricing pricing)
    {
        Stock = stock;
        _inventory = inventory;
        _purse = purse;
        _pricing = pricing;
    }

    public string Name => Stock.Definition.Name;

    public ShopStock Stock { get; }

    /// <summary>What buying <paramref name="quantity"/> copies would cost, and why it can't be done.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="quantity"/> is below 1.</exception>
    public TradeQuote QuoteBuy(ItemDefinition item, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        var total = PriceFormula.Total(Pricing.PriceOf(item), quantity);
        var refusal = !_inventory.HasRoomFor(item, quantity) ? TradeRefusal.StackFull
            : !_purse.CanSpend(total) ? TradeRefusal.ShortOfGold
            : (TradeRefusal?)null;
        return new TradeQuote(total, refusal);
    }

    /// <summary>What buying the piece would cost, and why it can't be done.</summary>
    public TradeQuote QuoteBuy(GearPiece piece)
    {
        var price = _pricing.PriceOf(piece);
        return new TradeQuote(price, _purse.CanSpend(price) ? null : TradeRefusal.ShortOfGold);
    }

    /// <summary>
    /// How many copies the gold and the stack's room allow (the picker's limit, decision 377); 0 when none, or when the
    /// stock doesn't hold the item.
    /// </summary>
    public int MostBuyable(ItemDefinition item)
    {
        if (!Stock.Stacks.Contains(item))
        {
            return 0;
        }

        var room = item.MaxStack - _inventory.QuantityOf(item.Id);
        var price = Pricing.PriceOf(item);
        var affordable = price == 0 ? room : _purse.Gold / price;
        return Math.Max(0, Math.Min(room, affordable));
    }

    /// <summary>Takes the total and adds the copies.</summary>
    /// <returns>False when the stock doesn't hold the item or the quote is refused; nothing changes then.</returns>
    public bool TryBuy(ItemDefinition item, int quantity)
    {
        if (!Stock.Stacks.Contains(item))
        {
            return false;
        }

        var quote = QuoteBuy(item, quantity);
        if (quote.IsRefused || !_purse.TrySpend(quote.Gold))
        {
            return false;
        }

        _inventory.Add(item, quantity);
        return true;
    }

    /// <summary>Takes the price and moves that very piece from the stock into the bag (decision 385).</summary>
    /// <returns>False when the stock doesn't hold the piece or the quote is refused; nothing changes then.</returns>
    public bool TryBuy(GearPiece piece)
    {
        if (!Stock.Pieces.Contains(piece))
        {
            return false;
        }

        var quote = QuoteBuy(piece);
        if (quote.IsRefused || !_purse.TrySpend(quote.Gold))
        {
            return false;
        }

        Stock.TryTake(piece);
        _inventory.AddGear(piece);
        return true;
    }
}
