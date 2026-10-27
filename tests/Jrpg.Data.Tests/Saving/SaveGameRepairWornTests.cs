using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Saving;
using Jrpg.Data.Content;
using Jrpg.Data.Saving;

namespace Jrpg.Data.Tests.Saving;

/// <summary>
/// Repairs what a member wears, slot by slot (ADR 0028), on the repository content with a ring and a helm added, which
/// it has none of yet.
/// </summary>
public sealed class SaveGameRepairWornTests
{
    private static readonly ContentCatalog _catalog = WithRingAndHelm(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));

    [Fact]
    public void Repair_UnknownItemInRingSlot_DropsIt()
    {
        var repaired = SaveGameRepair.Repair("slot_01", SaveWithSarxianWearing(new() { [EquipmentSlot.FirstRing] = "ghost_ring" }), _catalog);

        Assert.Empty(repaired.Save.Members[0].Worn);
        Assert.Empty(repaired.Save.Inventory);
        Assert.Contains("party member 'sarxian': dropped worn item 'ghost_ring', which no longer exists.", repaired.Changes);
    }

    [Fact]
    public void Repair_HelmSavedInChest_ReturnsItToInventory()
    {
        var repaired = SaveGameRepair.Repair("slot_01", SaveWithSarxianWearing(new() { [EquipmentSlot.Chest] = "leather_cap" }), _catalog);

        Assert.Empty(repaired.Save.Members[0].Worn);
        Assert.Equal([new InventoryEntry("leather_cap", 1)], repaired.Save.Inventory);
        Assert.Contains("party member 'sarxian': 'leather_cap' can no longer be worn as Chest, so it went back to the inventory.", repaired.Changes);
    }

    [Fact]
    public void Repair_RingInBothRingSlots_KeepsBoth()
    {
        var worn = new Dictionary<EquipmentSlot, string> { [EquipmentSlot.FirstRing] = "power_ring", [EquipmentSlot.SecondRing] = "power_ring" };

        var repaired = SaveGameRepair.Repair("slot_01", SaveWithSarxianWearing(worn), _catalog);

        Assert.Equal(worn, repaired.Save.Members[0].Worn);
        Assert.Empty(repaired.Changes);
    }

    private static ContentCatalog WithRingAndHelm(ContentCatalog catalog) => catalog with
    {
        Items = new Dictionary<string, ItemDefinition>(catalog.Items)
        {
            ["power_ring"] = Gear("power_ring", EquipmentKind.Ring),
            ["leather_cap"] = Gear("leather_cap", EquipmentKind.Head),
        },
    };

    private static ItemDefinition Gear(string id, EquipmentKind kind) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "Armor.png",
        MaxStack = 99,
        IsUsableFromMenu = false,
        Equipment = new EquipmentStats(kind),
    };

    private static SaveGame SaveWithSarxianWearing(Dictionary<EquipmentSlot, string> worn) => new()
    {
        SavedAt = new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero),
        PlayTime = TimeSpan.FromMinutes(30),
        Location = new MapLocation("start", new Point2D(10, 3), Facing.Down),
        Members = [new PartyMemberProgress("sarxian", Level: 3, Experience: 0, Hp: 100, worn, new Dictionary<string, int>())],
        ActiveMemberIds = ["sarxian"],
        Inventory = [],
        CompletedEvents = [],
        ObjectSwaps = [],
    };
}
