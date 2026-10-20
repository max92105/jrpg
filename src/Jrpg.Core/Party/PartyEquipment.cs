using Jrpg.Core.Items;

namespace Jrpg.Core.Party;

/// <summary>
/// Moves equipment between the inventory and party members. A worn copy is not in the inventory
/// (ADR 0003), so equipping takes a copy out and whatever it replaces goes back in.
/// </summary>
public sealed class PartyEquipment(Inventory inventory)
{
    /// <returns>
    /// True if the member wears the item afterwards; true without changes if they already wore it.
    /// False if the item isn't equipment, the member's class can't wear it, no copy is in the inventory,
    /// or the item it replaces doesn't fit back in the inventory. Nothing changes when it returns false.
    /// </returns>
    public bool TryEquip(PartyMember member, ItemDefinition item)
    {
        if (item.Equipment is null || !item.Equipment.CanBeWornBy(member.Definition.ClassId))
        {
            return false;
        }

        var slot = item.Equipment.Slot;
        var replaced = member.EquippedIn(slot);
        if (replaced?.Id == item.Id)
        {
            return true;
        }

        if ((replaced is not null && !inventory.HasRoomFor(replaced)) || !inventory.TryRemove(item.Id, 1))
        {
            return false;
        }

        if (replaced is not null)
        {
            inventory.Add(replaced, 1);
        }

        member.Wear(slot, item);
        return true;
    }

    /// <summary>What the member could put on in the slot: owned items for that slot their class can wear, in pickup order.</summary>
    /// <param name="member">Who would wear it.</param>
    /// <param name="slot">Weapon or armor.</param>
    /// <param name="items">Every item by id, to read the owned ones' slot and classes.</param>
    public IReadOnlyList<ItemDefinition> WearableFor(PartyMember member, EquipmentSlot slot, IReadOnlyDictionary<string, ItemDefinition> items) =>
        [.. inventory.Entries
            .Select(entry => items[entry.ItemId])
            .Where(item => item.Equipment is { } equipment && equipment.Slot == slot && equipment.CanBeWornBy(member.Definition.ClassId))];

    /// <returns>False if the slot is empty or the item's inventory stack is full. Nothing changes when it returns false.</returns>
    public bool TryUnequip(PartyMember member, EquipmentSlot slot)
    {
        var worn = member.EquippedIn(slot);
        if (worn is null || !inventory.HasRoomFor(worn))
        {
            return false;
        }

        inventory.Add(worn, 1);
        member.TakeOff(slot);
        return true;
    }
}
