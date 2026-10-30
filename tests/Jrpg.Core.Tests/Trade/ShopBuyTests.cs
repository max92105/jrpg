using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Trade;

namespace Jrpg.Core.Tests.Trade;

// Every purchase is quoted first (ADR 0031): the stack's room is named before the gold, and a refused one changes
// nothing.
public sealed class ShopBuyTests
{
    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").WithMaxStack(10).WithPrice(10).Build();
    private static readonly ItemDefinition _bomb = new ItemDefinitionBuilder().WithId("bomb").WithMaxStack(10).WithPrice(25).Build();
    private static readonly ItemDefinition _elixir = new ItemDefinitionBuilder().WithId("elixir").WithMaxStack(10).WithPrice(40).Build();

    private readonly Inventory _inventory = new();
    private readonly Purse _purse = new();
    private readonly Shop _shop;

    public ShopBuyTests()
    {
        _shop = new Shop(new ShopStock(new ShopDefinition("general", "General store", ["potion", "bomb"]), [_potion, _bomb]), _inventory, _purse);
    }

    [Fact]
    public void QuoteBuy_EnoughGold_QuotesTheTotal()
    {
        _purse.Add(100);

        var quote = _shop.QuoteBuy(_potion, 3);

        Assert.Equal(new TradeQuote(30, null), quote);
    }

    [Fact]
    public void QuoteBuy_ShortOfGold_RefusedShortOfGold()
    {
        _purse.Add(29);

        var quote = _shop.QuoteBuy(_potion, 3);

        Assert.Equal(new TradeQuote(30, TradeRefusal.ShortOfGold), quote);
    }

    [Fact]
    public void QuoteBuy_MoreThanTheStackTakes_RefusedStackFull()
    {
        _purse.Add(1000);
        _inventory.Add(_potion, 8);

        var quote = _shop.QuoteBuy(_potion, 3);

        Assert.Equal(TradeRefusal.StackFull, quote.Refusal);
    }

    // The bag is named before the purse.
    [Fact]
    public void QuoteBuy_StackFullAndShortOfGold_RefusedStackFull()
    {
        _inventory.Add(_potion, 8);

        var quote = _shop.QuoteBuy(_potion, 3);

        Assert.Equal(TradeRefusal.StackFull, quote.Refusal);
    }

    [Fact]
    public void QuoteBuy_ZeroQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _shop.QuoteBuy(_potion, 0));
    }

    [Fact]
    public void MostBuyable_GoldLimits_IsWhatTheGoldPays()
    {
        _purse.Add(35);

        Assert.Equal(3, _shop.MostBuyable(_potion));
    }

    [Fact]
    public void MostBuyable_RoomLimits_IsWhatTheStackTakes()
    {
        _purse.Add(1000);
        _inventory.Add(_potion, 7);

        Assert.Equal(3, _shop.MostBuyable(_potion));
    }

    [Fact]
    public void MostBuyable_StackFull_IsZero()
    {
        _purse.Add(1000);
        _inventory.Add(_potion, 10);

        Assert.Equal(0, _shop.MostBuyable(_potion));
    }

    [Fact]
    public void TryBuy_Quantity_AddsThatManyAndTakesTheTotal()
    {
        _purse.Add(100);

        var bought = _shop.TryBuy(_bomb, 2);

        Assert.True(bought);
        Assert.Equal(2, _inventory.QuantityOf("bomb"));
        Assert.Equal(50, _purse.Gold);
    }

    [Fact]
    public void TryBuy_Refused_ChangesNothing()
    {
        _purse.Add(49);

        var bought = _shop.TryBuy(_bomb, 2);

        Assert.False(bought);
        Assert.Equal(0, _inventory.QuantityOf("bomb"));
        Assert.Equal(49, _purse.Gold);
    }

    // Inventory.Add would silently drop what doesn't fit; the quote refuses first, so no gold goes for nothing.
    [Fact]
    public void TryBuy_StackFull_ChangesNothing()
    {
        _purse.Add(1000);
        _inventory.Add(_potion, 8);

        var bought = _shop.TryBuy(_potion, 3);

        Assert.False(bought);
        Assert.Equal(8, _inventory.QuantityOf("potion"));
        Assert.Equal(1000, _purse.Gold);
    }

    [Fact]
    public void MostBuyable_ItemNotInStock_IsZero()
    {
        _purse.Add(100);

        Assert.Equal(0, _shop.MostBuyable(_elixir));
    }

    [Fact]
    public void TryBuy_ItemNotInStock_ReturnsFalse()
    {
        _purse.Add(100);

        var bought = _shop.TryBuy(_elixir, 1);

        Assert.False(bought);
        Assert.Equal(100, _purse.Gold);
    }

    [Fact]
    public void QuoteBuy_ItemWithoutPrice_Throws()
    {
        var gift = new ItemDefinitionBuilder().WithId("gift").Build();

        Assert.Throws<InvalidOperationException>(() => _shop.QuoteBuy(gift, 1));
    }
}
