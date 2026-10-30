using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

// The session's shop reads the shared roll, the party's purse and the catalog's prices (ShopCatalog).
public sealed class GameSessionShopTests
{
    [Fact]
    public void ShopFor_FirstOpening_SellsTheRolledPiecesAtTheirPrice()
    {
        var session = GameSession.StartNew(ShopCatalog.Build(), "sarxian", new FixedSequenceRandom([1, 2]));
        session.Party.Purse.Add(500);

        var shop = session.ShopFor("store");
        var sword = shop.Stock.Pieces[0];
        var bought = shop.TryBuy(sword);

        Assert.True(bought);
        Assert.Equal(Rarity.Magic, sword.Rarity);
        Assert.Equal([sword], session.Inventory.Gear);
        Assert.Equal(500 - 120, session.Party.Purse.Gold);
        Assert.DoesNotContain(sword, session.ShopFor("store").Stock.Pieces);
    }

    [Fact]
    public void ShopFor_UnknownShop_ThrowsNamingIt()
    {
        var session = GameSession.StartNew(ShopCatalog.Build(), "sarxian", new FixedSequenceRandom());

        var exception = Assert.Throws<KeyNotFoundException>(() => session.ShopFor("bazaar"));

        Assert.Contains("bazaar", exception.Message, StringComparison.Ordinal);
    }
}
