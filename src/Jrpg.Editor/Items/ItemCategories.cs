using Jrpg.Core.Items;

namespace Jrpg.Editor.Items;

/// <summary>Which <see cref="ItemCategory"/> a record is.</summary>
internal static class ItemCategories
{
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
}
