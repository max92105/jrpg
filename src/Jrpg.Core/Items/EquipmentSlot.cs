namespace Jrpg.Core.Items;

/// <summary>
/// Where a member wears something (ADR 0028): the nine slots, the two ring slots holding the one ring kind. What an
/// item is worn as is its <see cref="EquipmentKind"/>.
/// </summary>
public enum EquipmentSlot
{
    Weapon,
    OffHand,
    Head,
    Chest,
    Hands,
    Feet,
    Amulet,
    FirstRing,
    SecondRing,
}
