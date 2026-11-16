using Jrpg.Core.Effects;
using Jrpg.Core.Items;

namespace Jrpg.Editor.Items;

/// <summary>
/// Which <see cref="ItemCategory"/> a record is, and the record a new item of each category starts from (decision 885).
/// </summary>
internal static class ItemCategories
{
    // Every gear record stacks to 99 (decision 879), and the page's starting values are its tunables (design.md,
    // section 1), placeholders the developer types over.
    private const int NewMaxStack = 99;
    private const int NewConsumablePower = 10;

    private static readonly ItemDefinition _newConsumable = NewRecord("Consumable.png") with
    {
        IsUsableFromMenu = true,
        Effect = new Effect(EffectKind.RestoreHp, NewConsumablePower, Targeting.SingleAlly),
    };

    private static readonly ItemDefinition _newEquipment = NewRecord("Armor.png") with
    {
        Equipment = new EquipmentStats(EquipmentKind.Chest),
    };

    private static readonly ItemDefinition _newMaterial = NewRecord("Item.png") with { IsMaterial = true };

    // A unique is equipment too, so its flag is read before the equipment; a material has neither.
    public static ItemCategory Of(ItemDefinition item)
    {
        if (item.IsMaterial)
        {
            return ItemCategory.Material;
        }

        if (item.IsUnique)
        {
            return ItemCategory.Unique;
        }

        return item.Equipment is null ? ItemCategory.Consumable : ItemCategory.Equipment;
    }

    /// <summary>
    /// A new item's record, its id and name left empty: a unique is the new equipment's with no fixed affix yet
    /// (decision 878), so <see cref="Of"/> reads every record back as its category.
    /// </summary>
    public static ItemDefinition StartingRecord(ItemCategory category) =>
        category switch
        {
            ItemCategory.Consumable => _newConsumable,
            ItemCategory.Equipment => _newEquipment,
            ItemCategory.Unique => _newEquipment with { IsUnique = true },
            ItemCategory.Material => _newMaterial,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Not an item category."),
        };

    // No price: a new item is never bought nor sold until one is typed (ADR 0031).
    private static ItemDefinition NewRecord(string iconFile) => new()
    {
        Id = "",
        Name = "",
        Description = "",
        IconFile = iconFile,
        MaxStack = NewMaxStack,
        IsUsableFromMenu = false,
    };
}
