using Jrpg.Core.Content;
using Jrpg.Core.Loot;
using Jrpg.Core.Trade;

namespace Jrpg.Core.Session;

/// <summary>
/// Every opened shop's stock by id (decision 385): rolled the first time a shop opens, kept while the leader stays at
/// the level it rolled at, and rolled again on the next opening once the leader has gained a level. Each gear entry of
/// the shop's list rolls into one piece through the shared <see cref="GearRoll"/>, in list order and with no luck
/// (decision 349); stacks are the catalog's items, never rolled.
/// </summary>
internal sealed class ShopStocks
{
    private readonly ContentCatalog _catalog;
    private readonly GearRoll _gearRoll;
    private readonly Dictionary<string, ShopStock> _stocks = [];

    /// <param name="catalog">The shops and their items.</param>
    /// <param name="gearRoll">The drops' roll, so a shop piece rolls as a drop does.</param>
    public ShopStocks(ContentCatalog catalog, GearRoll gearRoll)
    {
        _catalog = catalog;
        _gearRoll = gearRoll;
    }

    /// <summary>The shop's stock as it stands, rolled now when missing or stale.</summary>
    /// <param name="shopId">The shop in the catalog.</param>
    /// <param name="leaderLevel">The leader's level now; above the stock's roll level it rolls again.</param>
    /// <exception cref="KeyNotFoundException">No shop has <paramref name="shopId"/>.</exception>
    public ShopStock StockFor(string shopId, int leaderLevel)
    {
        if (!_stocks.TryGetValue(shopId, out var stock) || stock.RolledAtLevel < leaderLevel)
        {
            stock = Roll(_catalog.Shops[shopId], leaderLevel);
            _stocks[shopId] = stock;
        }

        return stock;
    }

    private ShopStock Roll(ShopDefinition shop, int leaderLevel)
    {
        var items = shop.ItemIds.Select(id => _catalog.Items[id]).ToList();
        var stacks = items.Where(item => item.Equipment is null).ToList();
        var pieces = items.Where(item => item.Equipment is not null).Select(item => _gearRoll.Roll(item, luckPermille: 0));
        return new ShopStock(shop, stacks, pieces, leaderLevel);
    }
}
