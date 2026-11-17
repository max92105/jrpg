using Jrpg.Core.Items;

namespace Jrpg.Editor.Items;

/// <summary>
/// The forms an item shows under its general fields, each null where its record has none: an equipment's for equipment
/// and uniques (decision 880), an effect's for a consumable (decision 881). Given to the item apart from its record,
/// since a duplicate's copies each form as typed.
/// </summary>
internal sealed record ItemForms(ItemEquipmentViewModel? Equipment, ItemEffectViewModel? Effect)
{
    /// <param name="item">The record whose equipment and effect the forms start from.</param>
    public static ItemForms Of(ItemDefinition item) =>
        new(
            item.Equipment is { } equipment ? ItemEquipmentViewModel.From(equipment) : null,
            item.Effect is { } effect ? ItemEffectViewModel.From(effect) : null);

    /// <summary>
    /// A copy of each form as typed, for a duplicated item: a half-typed number is copied, never read.
    /// </summary>
    public ItemForms CopyAsTyped() =>
        new(
            Equipment is null ? null : ItemEquipmentViewModel.CopyOf(Equipment),
            Effect is null ? null : ItemEffectViewModel.CopyOf(Effect));
}
