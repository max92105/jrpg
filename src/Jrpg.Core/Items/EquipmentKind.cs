namespace Jrpg.Core.Items;

/// <summary>
/// What an item is worn as (ADR 0028); <see cref="EquipmentFit"/> says which slots each kind fits. Only a weapon gives
/// the basic attack; the off-hand holds stats-only items (decision 322).
/// </summary>
public enum EquipmentKind
{
    Weapon,
    OffHand,
    Head,
    Chest,
    Hands,
    Feet,
    Amulet,
    Ring,
}
