using System.Collections.Immutable;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// One member as the Equipment page shows them, formatted once when the member or what they wear changes: their name,
/// what they wear in each of <see cref="Slots"/> (null for nothing), and their Max HP, Max MP, Attack and Defense.
/// </summary>
internal sealed record EquipmentSheet(string Name, ImmutableArray<ItemDefinition?> Worn, ImmutableArray<string> Stats)
{
    /// <summary>The page's slot rows, in order, before "Remove all".</summary>
    public static ImmutableArray<EquipmentSlot> Slots { get; } = [EquipmentSlot.Weapon, EquipmentSlot.Armor];

    public static EquipmentSheet Of(PartyMember member)
    {
        var stats = member.TotalStats;
        return new EquipmentSheet(
            member.Definition.Name,
            [.. Slots.Select(member.EquippedIn)],
            [NumberText.Of(member.Vitals.MaxHp), NumberText.Of(member.Vitals.MaxMp), NumberText.Of(stats.Attack), NumberText.Of(stats.Defense)]);
    }

    public static string LabelIdOf(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => MessageIds.SlotWeapon,
        EquipmentSlot.Armor => MessageIds.SlotArmor,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null),
    };
}
