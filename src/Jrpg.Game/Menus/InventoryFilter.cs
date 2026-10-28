using Jrpg.Core.Items;

namespace Jrpg.Game.Menus;

/// <summary>
/// Which owned stacks and gear pieces each inventory category shows, each in the order they were picked up; the page
/// lists the stacks, then the pieces (ADR 0029).
/// </summary>
internal static class InventoryFilter
{
    /// <param name="category">The category on show.</param>
    /// <param name="owned">The inventory's stacks, in pickup order.</param>
    /// <param name="items">Every item by id, from the catalog.</param>
    public static IReadOnlyList<InventoryEntry> SelectStacks(
        InventoryCategory category,
        IReadOnlyList<InventoryEntry> owned,
        IReadOnlyDictionary<string, ItemDefinition> items) =>
        [.. owned.Where(entry => Keeps(category, items[entry.ItemId]))];

    /// <param name="category">The category on show.</param>
    /// <param name="gear">The inventory's pieces, in pickup order.</param>
    public static IReadOnlyList<GearPiece> SelectPieces(InventoryCategory category, IReadOnlyList<GearPiece> gear) =>
        [.. gear.Where(piece => Keeps(category, piece.Base))];

    // A consumable is anything with an effect; the Armor tab is every piece of equipment that isn't a weapon.
    private static bool Keeps(InventoryCategory category, ItemDefinition item) => category switch
    {
        InventoryCategory.All => true,
        InventoryCategory.Consumables => item.Effect is not null,
        InventoryCategory.Weapons => item.Equipment?.Kind == EquipmentKind.Weapon,
        InventoryCategory.Armor => item.Equipment is { Kind: not EquipmentKind.Weapon },
        _ => throw new ArgumentOutOfRangeException(nameof(category), category, null),
    };
}
