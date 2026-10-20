using Jrpg.Core.Items;

namespace Jrpg.Game.Menus;

/// <summary>Which owned items each inventory category shows, in the order they were picked up.</summary>
internal static class InventoryFilter
{
    /// <param name="category">The category on show.</param>
    /// <param name="owned">The inventory's entries, in pickup order.</param>
    /// <param name="items">Every item by id, from the catalog.</param>
    public static IReadOnlyList<InventoryEntry> Select(
        InventoryCategory category,
        IReadOnlyList<InventoryEntry> owned,
        IReadOnlyDictionary<string, ItemDefinition> items) =>
        [.. owned.Where(entry => Keeps(category, items[entry.ItemId]))];

    // A consumable is anything with an effect; weapons and armor are told apart by the slot they're worn in.
    private static bool Keeps(InventoryCategory category, ItemDefinition item) => category switch
    {
        InventoryCategory.All => true,
        InventoryCategory.Consumables => item.Effect is not null,
        InventoryCategory.Weapons => item.Equipment?.Slot == EquipmentSlot.Weapon,
        InventoryCategory.Armor => item.Equipment?.Slot == EquipmentSlot.Armor,
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null),
    };
}
