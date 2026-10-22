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

    /// <summary>
    /// What the member could take from another member in the slot (decision 137): each other member's item there, active
    /// members first then reserve, in party order, that the member's class can wear and that isn't the very item they
    /// already wear. One entry per wearer, so two members wearing the same sword give two entries.
    /// </summary>
    public static IReadOnlyList<WornItem> TakeableFor(PartyMember member, EquipmentSlot slot, PartyState party)
    {
        var takeable = new List<WornItem>();
        foreach (var wearer in party.ActiveMembers.Concat(party.ReserveMembers))
        {
            if (wearer != member && wearer.EquippedIn(slot) is { } item && IsWorthTaking(item, member, slot))
            {
                takeable.Add(new WornItem(wearer, item));
            }
        }

        return takeable;
    }

    /// <summary>
    /// Moves the giver's item in the slot to the receiver (decision 137). The receiver's own item there goes back to the
    /// inventory, and the giver's slot is left empty.
    /// </summary>
    /// <returns>
    /// False if the giver is the receiver, the giver's slot is empty, the receiver's class can't wear the item, the receiver
    /// already wears the same item, or the receiver's own item doesn't fit back in the inventory. Nothing changes when it
    /// returns false.
    /// </returns>
    public bool TryTakeFrom(PartyMember giver, PartyMember receiver, EquipmentSlot slot)
    {
        var item = giver.EquippedIn(slot);
        if (giver == receiver || item is null || !IsWorthTaking(item, receiver, slot))
        {
            return false;
        }

        var replaced = receiver.EquippedIn(slot);
        if (replaced is not null && !inventory.HasRoomFor(replaced))
        {
            return false;
        }

        if (replaced is not null)
        {
            inventory.Add(replaced, 1);
        }

        giver.TakeOff(slot);
        receiver.Wear(slot, item);
        return true;
    }

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

    // The receiver can wear it, and it isn't the very item they already wear there, which taking would only shuffle.
    private static bool IsWorthTaking(ItemDefinition item, PartyMember receiver, EquipmentSlot slot) =>
        item.Equipment?.CanBeWornBy(receiver.Definition.ClassId) == true && item.Id != receiver.EquippedIn(slot)?.Id;
}
