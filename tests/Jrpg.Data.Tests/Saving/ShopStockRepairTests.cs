using Jrpg.Core.Items;
using Jrpg.Core.Trade;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

// Saved shop stocks repaired against the repository content (its general_store) and a one-affix pool ("power", 2..5): a
// stock piece is one of a kind, so one that no longer fits is dropped, and a value out of range is clamped.
public sealed class ShopStockRepairTests
{
    private static readonly SavedGear _sword = new("bronze_sword", Rarity.Magic, [new SavedAffix("power", 4)]);

    [Fact]
    public void Repair_KnownStock_KeepsIt()
    {
        var repaired = Repair([Stock(_sword)]);

        Assert.Equal([Stock(_sword)], repaired.Save.ShopStocks);
        Assert.Empty(repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownShop_DropsTheStock()
    {
        var repaired = Repair([new SavedShopStock("bazaar", 1, [_sword]), Stock(_sword)]);

        Assert.Equal([Stock(_sword)], repaired.Save.ShopStocks);
        Assert.Equal(["shop 'bazaar': dropped its stock, since the shop no longer exists."], repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownBase_DropsThePiece()
    {
        var repaired = Repair([Stock(Plain("excalibur"), _sword)]);

        Assert.Equal([Stock(_sword)], repaired.Save.ShopStocks);
        Assert.Equal(["shop 'general_store': dropped 'excalibur', which no longer exists."], repaired.Changes);
    }

    [Fact]
    public void Repair_BaseNoLongerEquipment_DropsThePiece()
    {
        var repaired = Repair([Stock(Plain("bomb"), _sword)]);

        Assert.Equal([Stock(_sword)], repaired.Save.ShopStocks);
        Assert.Equal(["shop 'general_store': dropped 'bomb', which is no longer gear."], repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownAffix_DropsThePiece()
    {
        var cursed = new SavedGear("bronze_sword", Rarity.Rare, [new SavedAffix("power", 4), new SavedAffix("vampiric", 3)]);

        var repaired = Repair([Stock(cursed, _sword)]);

        Assert.Equal([Stock(_sword)], repaired.Save.ShopStocks);
        Assert.Equal(["shop 'general_store': dropped 'bronze_sword' with affix 'vampiric', which no longer exists."], repaired.Changes);
    }

    [Fact]
    public void Repair_AffixOutOfRange_ClampsIt()
    {
        var strong = new SavedGear("bronze_sword", Rarity.Magic, [new SavedAffix("power", 9)]);

        var repaired = Repair([Stock(strong)]);

        Assert.Equal([Stock(_sword with { Affixes = [new SavedAffix("power", 5)] })], repaired.Save.ShopStocks);
        Assert.Equal(["shop 'general_store': affix 'power' on 'bronze_sword' brought within 2..5 (was 9)."], repaired.Changes);
    }

    private static SavedShopStock Stock(params SavedGear[] pieces) => new("general_store", 1, pieces);

    private static RepairedSave Repair(IReadOnlyList<SavedShopStock> stocks) =>
        SaveGameRepair.Repair("slot_01", RepairableSave with { ShopStocks = stocks }, CatalogWithPowerAffix);
}
