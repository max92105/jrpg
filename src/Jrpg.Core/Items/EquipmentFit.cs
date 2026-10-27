using System.Collections.Immutable;

namespace Jrpg.Core.Items;

/// <summary>
/// Which slots each kind of equipment fits (ADR 0028): a ring both ring slots, so the same ring may be worn twice
/// (decision 329); every other kind its own slot.
/// </summary>
public static class EquipmentFit
{
    /// <returns>
    /// The slots an item of <paramref name="kind"/> may be worn in, the first being where it goes by default.
    /// </returns>
    public static ImmutableArray<EquipmentSlot> SlotsFor(EquipmentKind kind) => kind switch
    {
        EquipmentKind.Weapon => [EquipmentSlot.Weapon],
        EquipmentKind.OffHand => [EquipmentSlot.OffHand],
        EquipmentKind.Head => [EquipmentSlot.Head],
        EquipmentKind.Chest => [EquipmentSlot.Chest],
        EquipmentKind.Hands => [EquipmentSlot.Hands],
        EquipmentKind.Feet => [EquipmentSlot.Feet],
        EquipmentKind.Amulet => [EquipmentSlot.Amulet],
        EquipmentKind.Ring => [EquipmentSlot.FirstRing, EquipmentSlot.SecondRing],
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown equipment kind."),
    };

    public static bool Fits(EquipmentKind kind, EquipmentSlot slot) => SlotsFor(kind).Contains(slot);
}
