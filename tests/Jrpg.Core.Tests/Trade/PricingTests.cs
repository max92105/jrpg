using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Trade;

namespace Jrpg.Core.Tests.Trade;

// A piece is worth its base item's price plus its affixes' gold values (decision 375).
public sealed class PricingTests
{
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).WithPrice(100).Build();
    private static readonly Pricing _pricing = new(new Dictionary<string, int> { ["power"] = 20, ["crit_chance"] = 35 }, new TradeRules { SellPercent = 50 });

    [Fact]
    public void PriceOf_PlainPiece_IsTheBasePrice()
    {
        Assert.Equal(100, _pricing.PriceOf(GearPiece.Plain(_sword)));
    }

    [Fact]
    public void PriceOf_RolledPiece_AddsItsAffixValues()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Rare, [new RolledAffix("power", StatKind.Power, 4), new RolledAffix("crit_chance", StatKind.CritChance, 2)]);

        Assert.Equal(155, _pricing.PriceOf(piece));
    }

    // The share of the whole price, base and affixes (decision 375 as read): 155 at 50 % is 77.
    [Fact]
    public void SalePriceOf_RolledPiece_SharesBasePlusAffixValues()
    {
        var piece = GearPiece.Rolled(_sword, Rarity.Rare, [new RolledAffix("power", StatKind.Power, 4), new RolledAffix("crit_chance", StatKind.CritChance, 2)]);

        Assert.Equal(77, _pricing.SalePriceOf(piece));
    }

    [Fact]
    public void SalePriceOf_UnpricedItem_IsZero()
    {
        var gift = new ItemDefinitionBuilder().WithId("gift").Build();

        Assert.Equal(0, _pricing.SalePriceOf(gift));
    }

    [Fact]
    public void PriceOf_Item_IsItsPrice()
    {
        Assert.Equal(100, Pricing.PriceOf(_sword));
    }

    [Fact]
    public void PriceOf_ItemWithoutPrice_Throws()
    {
        var gift = new ItemDefinitionBuilder().WithId("gift").Build();

        Assert.Throws<InvalidOperationException>(() => Pricing.PriceOf(gift));
    }
}
