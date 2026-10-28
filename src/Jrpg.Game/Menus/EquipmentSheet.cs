using System.Collections.Immutable;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// One member as the Equipment page shows them, formatted once when the member or what they wear changes: their name,
/// and what they wear in each of <see cref="Slots"/> (null for nothing).
/// </summary>
internal sealed record EquipmentSheet(string Name, ImmutableArray<GearPiece?> Worn)
{
    /// <summary>The page's slot rows, in order, before "Remove all" (ADR 0028); both ring slots read "Ring".</summary>
    public static ImmutableArray<EquipmentSlot> Slots { get; } =
    [
        EquipmentSlot.Weapon,
        EquipmentSlot.OffHand,
        EquipmentSlot.Head,
        EquipmentSlot.Chest,
        EquipmentSlot.Hands,
        EquipmentSlot.Feet,
        EquipmentSlot.Amulet,
        EquipmentSlot.FirstRing,
        EquipmentSlot.SecondRing,
    ];

    public static EquipmentSheet Of(PartyMember member) => new(member.Definition.Name, [.. Slots.Select(member.EquippedIn)]);

    public static string LabelIdOf(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => MessageIds.SlotWeapon,
        EquipmentSlot.OffHand => MessageIds.SlotOffHand,
        EquipmentSlot.Head => MessageIds.SlotHead,
        EquipmentSlot.Chest => MessageIds.SlotChest,
        EquipmentSlot.Hands => MessageIds.SlotHands,
        EquipmentSlot.Feet => MessageIds.SlotFeet,
        EquipmentSlot.Amulet => MessageIds.SlotAmulet,
        EquipmentSlot.FirstRing or EquipmentSlot.SecondRing => MessageIds.SlotRing,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null),
    };
}
