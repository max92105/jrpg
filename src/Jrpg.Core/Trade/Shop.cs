using Jrpg.Core.Items;
using Jrpg.Core.Party;

namespace Jrpg.Core.Trade;

/// <summary>
/// Buying from one shop's stock (ADR 0031). Each purchase is quoted first; a refused one, or an item the stock doesn't
/// hold, changes nothing. The stack's room is checked before the gold, so a full bag is named before an empty purse.
/// </summary>
public sealed class Shop
{
    private readonly Inventory _inventory;
    private readonly Purse _purse;

    /// <param name="stock">What the shop sells.</param>
    /// <param name="inventory">The party's: where bought copies go.</param>
    /// <param name="purse">The party's gold.</param>
    internal Shop(ShopStock stock, Inventory inventory, Purse purse)
    {
        Stock = stock;
        _inventory = inventory;
        _purse = purse;
    }

    public string Name => Stock.Definition.Name;

    public ShopStock Stock { get; }

    /// <summary>What buying <paramref name="quantity"/> copies would cost, and why it can't be done.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="quantity"/> is below 1.</exception>
    public TradeQuote QuoteBuy(ItemDefinition item, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        var total = PriceOf(item) * quantity;
        var refusal = !_inventory.HasRoomFor(item, quantity) ? TradeRefusal.StackFull
            : !_purse.CanSpend(total) ? TradeRefusal.ShortOfGold
            : (TradeRefusal?)null;
        return new TradeQuote(total, refusal);
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
        var price = PriceOf(item);
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

    // A stocked item always has a price: the loader refuses a shop listing one without (ADR 0031).
    private static int PriceOf(ItemDefinition item) =>
        item.Price ?? throw new InvalidOperationException($"Item '{item.Id}' has no price, so no shop can sell it.");
}
