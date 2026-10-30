using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Trade;

namespace Jrpg.Core.Tests.Trade;

// A sale pays the sell share of the price, rounded down (decision 375); a thing with no price, or a share worth
// nothing, is not for sale (decision 378); gold past the cap is lost, never refused.
public sealed class ItemSalesTests
{
    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").WithPrice(10).Build();
    private static readonly ItemDefinition _pebble = new ItemDefinitionBuilder().WithId("pebble").WithPrice(1).Build();
    private static readonly ItemDefinition _gift = new ItemDefinitionBuilder().WithId("gift").Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).WithPrice(100).Build();
    private static readonly GearPiece _magicSword = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);

    private readonly Inventory _inventory = new();
    private readonly Purse _purse = new();
    private readonly ItemSales _sales;

    public ItemSalesTests()
    {
        _sales = new ItemSales(_inventory, _purse, new Pricing(new Dictionary<string, int> { ["power"] = 20 }, new TradeRules { SellPercent = 50 }));
    }

    [Fact]
    public void QuoteSale_PricedStack_QuotesTheShareTimesTheQuantity()
    {
        Assert.Equal(new TradeQuote(15, null), _sales.QuoteSale(_potion, 3));
    }

    [Fact]
    public void QuoteSale_UnpricedItem_RefusedNotForSale()
    {
        Assert.Equal(TradeRefusal.NotForSale, _sales.QuoteSale(_gift, 1).Refusal);
    }

    // 1 at 50 % rounds down to nothing: the shop won't take it for free.
    [Fact]
    public void QuoteSale_ShareRoundsToZero_RefusedNotForSale()
    {
        Assert.Equal(TradeRefusal.NotForSale, _sales.QuoteSale(_pebble, 5).Refusal);
    }

    [Fact]
    public void QuoteSale_ZeroQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _sales.QuoteSale(_potion, 0));
    }

    // Half of 120, the base and the affix together (decision 375 as read).
    [Fact]
    public void QuoteSale_RolledPiece_SharesBasePlusAffixValues()
    {
        Assert.Equal(new TradeQuote(60, null), _sales.QuoteSale(_magicSword));
    }

    // A piece whose base has no price is not for sale either, instead of a crash from the base's missing price.
    [Fact]
    public void QuoteSale_PieceWithUnpricedBase_RefusedNotForSale()
    {
        var heirloom = new ItemDefinitionBuilder().WithId("heirloom").AsWeapon(24).Build();

        Assert.Equal(TradeRefusal.NotForSale, _sales.QuoteSale(GearPiece.Plain(heirloom)).Refusal);
    }

    [Fact]
    public void TrySell_PieceNotOwned_ChangesNothing()
    {
        _inventory.AddGear(GearPiece.Plain(_sword));

        var sold = _sales.TrySell(_magicSword);

        Assert.False(sold);
        Assert.Equal([GearPiece.Plain(_sword)], _inventory.Gear);
        Assert.Equal(0, _purse.Gold);
    }

    [Fact]
    public void MostSellable_OwnedStack_IsTheCount()
    {
        _inventory.Add(_potion, 7);

        Assert.Equal(7, _sales.MostSellable(_potion));
    }

    [Fact]
    public void MostSellable_NotForSale_IsZero()
    {
        _inventory.Add(_gift, 7);

        Assert.Equal(0, _sales.MostSellable(_gift));
    }

    [Fact]
    public void TrySell_StackQuantity_TakesThemAndAddsGold()
    {
        _inventory.Add(_potion, 7);

        var sold = _sales.TrySell(_potion, 3);

        Assert.True(sold);
        Assert.Equal(4, _inventory.QuantityOf("potion"));
        Assert.Equal(15, _purse.Gold);
    }

    [Fact]
    public void TrySell_Piece_RemovesThatPieceAndAddsGold()
    {
        _inventory.AddGear(_magicSword);
        _inventory.AddGear(GearPiece.Plain(_sword));

        var sold = _sales.TrySell(_magicSword);

        Assert.True(sold);
        Assert.Equal([GearPiece.Plain(_sword)], _inventory.Gear);
        Assert.Equal(60, _purse.Gold);
    }

    // One overflow rule: the sale goes through and the gold past the cap is lost.
    [Fact]
    public void TrySell_PurseNearlyFull_SellsAndLosesTheGoldPastMax()
    {
        _purse.Add(Purse.MaxGold - 2);
        _inventory.Add(_potion, 1);

        var sold = _sales.TrySell(_potion, 1);

        Assert.True(sold);
        Assert.Equal(0, _inventory.QuantityOf("potion"));
        Assert.Equal(Purse.MaxGold, _purse.Gold);
    }

    [Fact]
    public void TrySell_ItemNotOwned_ReturnsFalse()
    {
        _inventory.Add(_potion, 2);

        var sold = _sales.TrySell(_potion, 3);

        Assert.False(sold);
        Assert.Equal(2, _inventory.QuantityOf("potion"));
        Assert.Equal(0, _purse.Gold);
    }

    [Fact]
    public void TrySell_NotForSale_ChangesNothing()
    {
        _inventory.Add(_gift, 2);

        var sold = _sales.TrySell(_gift, 1);

        Assert.False(sold);
        Assert.Equal(2, _inventory.QuantityOf("gift"));
    }
}
