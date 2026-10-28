using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Saving;
using Jrpg.Core.Stats;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

// Saved pieces repaired against the repository content and a one-affix pool ("power"): bombs are consumables, and a
// leather cap is worn on the head.
public sealed class GearRepairTests
{
    [Fact]
    public void Repair_UnknownBase_DropsPiece()
    {
        var repaired = Repair(RepairableSave with { Gear = [Plain("excalibur"), Plain("bronze_sword")] });

        Assert.Equal([Plain("bronze_sword")], repaired.Save.Gear);
        Assert.Equal(["the inventory: dropped gear 'excalibur', which no longer exists."], repaired.Changes);
    }

    [Fact]
    public void Repair_OwnedBaseNoLongerEquipment_GoesBackToTheStacks()
    {
        var repaired = Repair(RepairableSave with { Gear = [Plain("bomb")] });

        Assert.Empty(repaired.Save.Gear);
        Assert.Contains(new InventoryEntry("bomb", 1), repaired.Save.Inventory);
        Assert.Equal(["the inventory: 'bomb' is no longer gear, so it went back to the stacks."], repaired.Changes);
    }

    // The owned pieces keep their order; a worn piece taken off joins them last.
    [Fact]
    public void Repair_WornKindNoLongerFits_MovesToGear()
    {
        var worn = Wearing((EquipmentSlot.Chest, "leather_cap"));

        var repaired = Repair(RepairableSave with { Members = [Sarxian with { Worn = worn }, Jazog], Gear = [Plain("bronze_sword")] });

        Assert.Empty(repaired.Save.Members[0].Worn);
        Assert.Equal([Plain("bronze_sword"), Plain("leather_cap")], repaired.Save.Gear);
    }

    [Fact]
    public void Repair_AffixNoLongerInThePool_DropsItAndReports()
    {
        var sword = new SavedGear("bronze_sword", Rarity.Magic, [new SavedAffix("vampiric", 3), new SavedAffix("power", 4)]);

        var repaired = Repair(RepairableSave with { Gear = [sword] });

        Assert.Equal([sword with { Affixes = [new SavedAffix("power", 4)] }], repaired.Save.Gear);
        Assert.Equal(["the inventory: dropped affix 'vampiric' from 'bronze_sword', which no longer exists."], repaired.Changes);
    }

    // The pool's "power" rolls 2..5; a value saved under older numbers is clamped and reported (decision 360).
    [Fact]
    public void Repair_ValueAboveRange_ClampsToMaxAndReports()
    {
        var repaired = Repair(RepairableSave with { Gear = [new SavedGear("bronze_sword", Rarity.Magic, [new SavedAffix("power", 7)])] });

        Assert.Equal([new SavedAffix("power", 5)], repaired.Save.Gear[0].Affixes);
        Assert.Equal(["the inventory: affix 'power' on 'bronze_sword' brought within 2..5 (was 7)."], repaired.Changes);
    }

    [Fact]
    public void Repair_ValueBelowRange_ClampsToMinAndReports()
    {
        var repaired = Repair(RepairableSave with { Gear = [new SavedGear("bronze_sword", Rarity.Magic, [new SavedAffix("power", 1)])] });

        Assert.Equal([new SavedAffix("power", 2)], repaired.Save.Gear[0].Affixes);
        Assert.Equal(["the inventory: affix 'power' on 'bronze_sword' brought within 2..5 (was 1)."], repaired.Changes);
    }

    [Fact]
    public void Repair_PieceThatFits_UnchangedAndNotReported()
    {
        var sword = new SavedGear("bronze_sword", Rarity.Magic, [new SavedAffix("power", 5)]);

        var repaired = Repair(RepairableSave with { Gear = [sword] });

        Assert.Equal([sword], repaired.Save.Gear);
        Assert.Empty(repaired.Changes);
    }

    // A worn piece's affixes count toward the max the saved HP is held within: sarxian, a level 3 warlock, has 203 HP
    // with nothing worn and 221 with a +10 Constitution affix (ADR 0026). The unknown affix leaves before the sheet.
    [Fact]
    public void Repair_WornAffixRaisingMaxHp_KeepsHpAndDropsTheUnknownAffix()
    {
        var sturdy = new AffixDefinition { Id = "sturdy", Stat = StatKind.Constitution, Min = 1, Max = 10, Kinds = [EquipmentKind.Ring], Weight = 10 };
        var catalog = CatalogWithPowerAffix with { Affixes = new Dictionary<string, AffixDefinition>(CatalogWithPowerAffix.Affixes) { ["sturdy"] = sturdy } };
        var ring = new SavedGear("power_ring", Rarity.Rare, [new SavedAffix("sturdy", 10), new SavedAffix("vampiric", 3)]);
        var worn = new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.FirstRing] = ring };

        var repaired = SaveGameRepair.Repair("slot_01", RepairableSave with { Members = [Sarxian with { Hp = 215, Worn = worn }, Jazog] }, catalog);

        Assert.Equal(215, repaired.Save.Members[0].Hp);
        Assert.Equal([new SavedAffix("sturdy", 10)], repaired.Save.Members[0].Worn[EquipmentSlot.FirstRing].Affixes);
        Assert.Equal(["party member 'sarxian': dropped affix 'vampiric' from 'power_ring', which no longer exists."], repaired.Changes);
    }

    private static RepairedSave Repair(SaveGame save) => SaveGameRepair.Repair("slot_01", save, CatalogWithPowerAffix);
}
