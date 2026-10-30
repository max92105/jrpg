using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Trade;

namespace Jrpg.Core.Tests.Trade;

// A stock piece is one of a kind (decision 385): buying it moves that very piece into the bag and out of the stock.
public sealed class ShopPieceTests
{
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).WithPrice(100).Build();
    private static readonly GearPiece _magicSword = GearPiece.Rolled(_sword, Rarity.Magic, [new RolledAffix("power", StatKind.Power, 4)]);
    private static readonly GearPiece _plainSword = GearPiece.Plain(_sword);

    private readonly Inventory _inventory = new();
    private readonly Purse _purse = new();
    private readonly ShopStock _stock = new(new ShopDefinition("smithy", "Smithy", ["bronze_sword"]), [], [_magicSword], rolledAtLevel: 1);
    private readonly Shop _shop;

    public ShopPieceTests()
    {
        _shop = new Shop(_stock, _inventory, _purse, new Pricing(new Dictionary<string, int> { ["power"] = 20 }, new TradeRules()));
    }

    [Fact]
    public void QuoteBuy_Piece_QuotesItsPrice()
    {
        _purse.Add(200);

        Assert.Equal(new TradeQuote(120, null), _shop.QuoteBuy(_magicSword));
    }

    [Fact]
    public void QuoteBuy_PieceShortOfGold_Refused()
    {
        _purse.Add(119);

        Assert.Equal(new TradeQuote(120, TradeRefusal.ShortOfGold), _shop.QuoteBuy(_magicSword));
    }

    [Fact]
    public void TryBuy_Piece_MovesThatPieceFromStockToInventory()
    {
        _purse.Add(200);

        var bought = _shop.TryBuy(_magicSword);

        Assert.True(bought);
        Assert.Equal([_magicSword], _inventory.Gear);
        Assert.Empty(_stock.Pieces);
        Assert.Equal(80, _purse.Gold);
    }

    [Fact]
    public void TryBuy_PieceShortOfGold_ChangesNothing()
    {
        _purse.Add(119);

        var bought = _shop.TryBuy(_magicSword);

        Assert.False(bought);
        Assert.Empty(_inventory.Gear);
        Assert.Equal([_magicSword], _stock.Pieces);
        Assert.Equal(119, _purse.Gold);
    }

    [Fact]
    public void TryBuy_PieceNotInStock_ReturnsFalse()
    {
        _purse.Add(200);

        var bought = _shop.TryBuy(_plainSword);

        Assert.False(bought);
        Assert.Equal(200, _purse.Gold);
        Assert.Empty(_inventory.Gear);
        Assert.Equal([_magicSword], _stock.Pieces);
    }
}
