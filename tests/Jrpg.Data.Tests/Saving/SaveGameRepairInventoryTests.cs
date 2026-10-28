using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Saving;
using Jrpg.Core.World;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

/// <summary>
/// Repairs the inventory, the location and the world's events and swaps of a save against the repository content:
/// potions stack to 99.
/// </summary>
public sealed class SaveGameRepairInventoryTests
{
    private const string Slot = "slot_01";

    [Fact]
    public void Repair_UnknownItem_DropsEntry()
    {
        var repaired = Repair(RepairableSave with { Inventory = [new InventoryEntry("megalixir", 2), new InventoryEntry("potion", 4)] });

        Assert.Equal([new InventoryEntry("potion", 4)], repaired.Save.Inventory);
        Assert.Single(repaired.Changes);
    }

    [Fact]
    public void Repair_QuantityAboveMaxStack_Clamps()
    {
        var repaired = Repair(RepairableSave with { Inventory = [new InventoryEntry("potion", 150)] });

        Assert.Equal([new InventoryEntry("potion", 99)], repaired.Save.Inventory);
        Assert.Contains("kept 99 x 'potion' of 150; the stack holds 99.", repaired.Changes);
    }

    [Fact]
    public void Repair_ZeroQuantityAndDuplicateEntries_DropsAndMerges()
    {
        var repaired = Repair(RepairableSave with { Inventory = [new InventoryEntry("bomb", 0), new InventoryEntry("potion", 4), new InventoryEntry("potion", 2)] });

        Assert.Equal([new InventoryEntry("potion", 6)], repaired.Save.Inventory);
    }

    [Fact]
    public void Repair_CompletedEventOnUnknownMap_DropsIt()
    {
        var events = new[] { new MapEventKey("castle", "gate_0"), new MapEventKey("start", "removed_0"), new MapEventKey("start", "0bff6243_0") };

        var repaired = Repair(RepairableSave with { CompletedEvents = events });

        Assert.Equal([new MapEventKey("start", "0bff6243_0")], repaired.Save.CompletedEvents);
        Assert.Equal(2, repaired.Changes.Count);
    }

    [Fact]
    public void Repair_SwapToUnknownObject_DropsIt()
    {
        var swaps = new[] { new ObjectSwap("start", "0bff6243", "golden_chest"), new ObjectSwap("start", "missing_uid", "chest_opened") };

        var repaired = Repair(RepairableSave with { ObjectSwaps = swaps });

        Assert.Empty(repaired.Save.ObjectSwaps);
        Assert.Equal(2, repaired.Changes.Count);
    }

    [Fact]
    public void Repair_UnknownLocationMap_UsesNewGameStart()
    {
        var repaired = Repair(RepairableSave with { Location = new MapLocation("castle", new Point2D(4, 4), Facing.Up) });

        Assert.Equal(Catalog.NewGame.StartLocation, repaired.Save.Location);
        Assert.Contains("map 'castle' no longer exists, so the party starts at the new game's start.", repaired.Changes);
    }

    [Fact]
    public void Repair_QuantitiesSummingPastIntMax_ClampsToMaxStack()
    {
        var repaired = Repair(RepairableSave with { Inventory = [new InventoryEntry("potion", int.MaxValue), new InventoryEntry("potion", 1)] });

        Assert.Equal([new InventoryEntry("potion", 99)], repaired.Save.Inventory);
    }

    [Fact]
    public void Repair_SwapOnUnknownMap_DropsItNamingTheMap()
    {
        var repaired = Repair(RepairableSave with { ObjectSwaps = [new ObjectSwap("castle", "0bff6243", "chest_opened")] });

        Assert.Empty(repaired.Save.ObjectSwaps);
        Assert.Equal(["forgot that '0bff6243' on map 'castle' became 'chest_opened': the map no longer exists."], repaired.Changes);
    }

    [Fact]
    public void Repair_RemovedEventOnKnownMap_NamesTheEvent()
    {
        var repaired = Repair(RepairableSave with { CompletedEvents = [new MapEventKey("start", "removed_0")] });

        Assert.Equal(["forgot done event 'removed_0' on map 'start': the event no longer exists."], repaired.Changes);
    }

    private static RepairedSave Repair(SaveGame save) => SaveGameRepair.Repair(Slot, save, Catalog);
}
