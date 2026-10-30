using Jrpg.Core.Items;
using Jrpg.Core.Session;
using Jrpg.Core.Tests.Randomness;
using static Jrpg.Core.Tests.Builders.ShopCatalog;

namespace Jrpg.Core.Tests.Session;

// ShopCatalog's Magic-only table: each gear entry draws exactly its affix's value, in the shop's order; the stacks draw
// nothing. The strict random throws on a draw the test didn't script.
public sealed class ShopStocksTests
{
    [Fact]
    public void StockFor_FirstOpening_RollsOnePiecePerGearEntryInOrder()
    {
        var stocks = Stocks([1, 2]);

        var stock = stocks.StockFor("store", leaderLevel: 1);

        GearPiece[] expected = [Magic(Sword, Power, 3), Magic(Ring, Agility, 4)];
        Assert.Equal(expected, stock.Pieces);
        Assert.Equal(1, stock.RolledAtLevel);
    }

    [Fact]
    public void StockFor_Stacks_AreNeverRolled()
    {
        var stock = Stocks([1, 2]).StockFor("store", leaderLevel: 1);

        Assert.Equal([Potion], stock.Stacks);
    }

    [Fact]
    public void StockFor_OpenedAgain_SameStockAndNoDraw()
    {
        var stocks = Stocks([1, 2]);
        var first = stocks.StockFor("store", leaderLevel: 1);

        var again = stocks.StockFor("store", leaderLevel: 1);

        Assert.Same(first, again);
    }

    [Fact]
    public void StockFor_LeaderGainedALevel_RollsAgain()
    {
        var stocks = Stocks([1, 2, 3, 0]);
        stocks.StockFor("store", leaderLevel: 1);

        var rerolled = stocks.StockFor("store", leaderLevel: 2);

        GearPiece[] expected = [Magic(Sword, Power, 5), Magic(Ring, Agility, 2)];
        Assert.Equal(expected, rerolled.Pieces);
        Assert.Equal(2, rerolled.RolledAtLevel);
    }

    // The sword bought at level 1 is gone from every opening at that level, and comes back as a new roll at level 2.
    [Fact]
    public void StockFor_PieceBought_StaysGoneUntilTheReroll()
    {
        var stocks = Stocks([1, 2, 3, 0]);
        var stock = stocks.StockFor("store", leaderLevel: 1);
        Assert.True(stock.TryTake(Magic(Sword, Power, 3)));

        var again = stocks.StockFor("store", leaderLevel: 1);
        var rerolled = stocks.StockFor("store", leaderLevel: 2);

        Assert.Equal([Magic(Ring, Agility, 4)], again.Pieces);
        Assert.Equal([Magic(Sword, Power, 5), Magic(Ring, Agility, 2)], rerolled.Pieces);
    }

    [Fact]
    public void StockFor_UnknownShop_ThrowsNamingIt()
    {
        var exception = Assert.Throws<KeyNotFoundException>(() => Stocks([]).StockFor("bazaar", leaderLevel: 1));

        Assert.Contains("bazaar", exception.Message, StringComparison.Ordinal);
    }

    private static ShopStocks Stocks(int[] draws)
    {
        var catalog = Build();
        return new ShopStocks(catalog, GameSession.GearRollFor(catalog, new FixedSequenceRandom(draws)));
    }
}
