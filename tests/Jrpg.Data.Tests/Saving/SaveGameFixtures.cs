using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Saving;
using Jrpg.Core.World;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Saving;

/// <summary>
/// The saves the store and repair tests share. Both saves fit the repository content, so reading or repairing them
/// unchanged reports nothing.
/// </summary>
internal static class SaveGameFixtures
{
    public static readonly ContentCatalog Catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    /// <summary>The save the store tests write and read back.</summary>
    public static readonly SaveGame StoredSave = new()
    {
        SavedAt = new DateTimeOffset(2026, 10, 17, 20, 30, 15, TimeSpan.FromHours(-4)),
        PlayTime = TimeSpan.FromSeconds(4_321.25),
        Location = new MapLocation("start", new Point2D(10.25, 3.5), Facing.Left),
        Members =
        [
            new PartyMemberProgress("sarxian", Level: 3, Experience: 42, Hp: 77, Worn: new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Weapon] = "bronze_sword" }, LastingAmounts: new Dictionary<string, int>()),
            new PartyMemberProgress("jazog", Level: 1, Experience: 0, Hp: 120, Worn: new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Chest] = "wizard_robe" }, LastingAmounts: new Dictionary<string, int>()),
        ],
        ActiveMemberIds = ["sarxian", "jazog"],
        Inventory = [new InventoryEntry("potion", 4), new InventoryEntry("bomb", 3)],
        CompletedEvents = [new MapEventKey("start", "0bff6243_0")],
        ObjectSwaps = [new ObjectSwap("start", "0bff6243", "chest_opened")],
    };

    public static readonly PartyMemberProgress Sarxian = new("sarxian", Level: 3, Experience: 42, Hp: 150, Wearing((EquipmentSlot.Weapon, "bronze_sword"), (EquipmentSlot.Chest, "chain_mail")), LastingAmounts: new Dictionary<string, int>());

    public static readonly PartyMemberProgress Jazog = new("jazog", Level: 1, Experience: 0, Hp: 100, Wearing((EquipmentSlot.Chest, "wizard_robe")), LastingAmounts: new Dictionary<string, int>());

    /// <summary>The save the repair tests change one part of: <see cref="Sarxian"/> and <see cref="Jazog"/>.</summary>
    public static readonly SaveGame RepairableSave = new()
    {
        SavedAt = new DateTimeOffset(2026, 10, 17, 20, 30, 0, TimeSpan.Zero),
        PlayTime = TimeSpan.FromMinutes(30),
        Location = new MapLocation("start", new Point2D(10, 3), Facing.Down),
        Members = [Sarxian, Jazog],
        ActiveMemberIds = ["sarxian", "jazog"],
        Inventory = [new InventoryEntry("potion", 4)],
        CompletedEvents = [new MapEventKey("start", "0bff6243_0")],
        ObjectSwaps = [new ObjectSwap("start", "0bff6243", "chest_opened")],
    };

    public static Dictionary<EquipmentSlot, string> Wearing(params (EquipmentSlot Slot, string ItemId)[] worn) =>
        worn.ToDictionary(entry => entry.Slot, entry => entry.ItemId);
}
