using Jrpg.Core.Items;
using Jrpg.Core.Loot;

namespace Jrpg.Editor.Items;

/// <summary>
/// The forms an item shows under its general fields, each null where its record has none: an equipment's for equipment
/// and uniques (decision 880), an effect's for a consumable (decision 881), the fixed affixes' for a unique (decision
/// 882). Given to the item apart from its record, since a duplicate's copies each form as typed.
/// </summary>
internal sealed record ItemForms(ItemEquipmentViewModel? Equipment, ItemEffectViewModel? Effect, UniqueAffixesViewModel? Unique)
{
    /// <param name="item">The record whose equipment, effect and fixed affixes the forms start from.</param>
    /// <param name="pool">The draft's affixes by id, which a unique's rows offer and price.</param>
    public static ItemForms Of(ItemDefinition item, IReadOnlyDictionary<string, AffixDefinition> pool) =>
        new(
            item.Equipment is { } equipment ? ItemEquipmentViewModel.From(equipment) : null,
            item.Effect is { } effect ? ItemEffectViewModel.From(effect) : null,
            item.IsUnique ? UniqueAffixesViewModel.From(item.FixedAffixes, pool) : null);

    /// <summary>
    /// A copy of each form as typed, for a duplicated item: a half-typed number is copied, never read.
    /// </summary>
    public ItemForms CopyAsTyped() =>
        new(
            Equipment is null ? null : ItemEquipmentViewModel.CopyOf(Equipment),
            Effect is null ? null : ItemEffectViewModel.CopyOf(Effect),
            Unique is null ? null : UniqueAffixesViewModel.CopyOf(Unique));
}
