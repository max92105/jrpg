using Jrpg.Core.Items;

namespace Jrpg.Core.Party;

/// <summary>
/// Moves equipment between the inventory and party members. A worn copy is not in the inventory
/// (ADR 0003), so equipping takes a copy out and whatever it replaces goes back in.
/// </summary>
public sealed class PartyEquipment(Inventory inventory)
{
    /// <summary>Equips the item in the first slot its kind fits: a ring in the first ring slot.</summary>
    /// <returns>
    /// As <see cref="TryEquip(PartyMember, ItemDefinition, EquipmentSlot)"/>; false for a non-equipment item.
    /// </returns>
    public bool TryEquip(PartyMember member, ItemDefinition item) =>
        item.Equipment is { } equipment && TryEquip(member, item, EquipmentFit.SlotsFor(equipment.Kind)[0]);

    /// <returns>
    /// True if the member wears the item in the slot afterwards; true without changes if they already wore it
    /// there. False if the item isn't equipment whose kind fits the slot (ADR 0028), no copy is in the inventory, or
    /// the item it replaces doesn't fit back in the inventory. Nothing changes when it returns false.
    /// </returns>
    public bool TryEquip(PartyMember member, ItemDefinition item, EquipmentSlot slot)
    {
        if (item.Equipment is null || !EquipmentFit.Fits(item.Equipment.Kind, slot))
        {
            return false;
        }

        var piece = GearPiece.Plain(item);
        var replaced = member.EquippedIn(slot);
        if (piece.Equals(replaced))
        {
            return true;
        }

        if ((replaced is not null && !inventory.HasRoomFor(replaced.Base)) || !inventory.TryRemove(item.Id, 1))
        {
            return false;
        }

        if (replaced is not null)
        {
            inventory.Add(replaced.Base, 1);
        }

        member.Wear(slot, piece);
        return true;
    }

    /// <summary>What any member could put on in the slot: owned items for that slot, in pickup order.</summary>
    /// <param name="slot">The slot to list for.</param>
    /// <param name="items">Every item by id, to read the owned ones' kind.</param>
    public IReadOnlyList<ItemDefinition> OwnedFor(EquipmentSlot slot, IReadOnlyDictionary<string, ItemDefinition> items) =>
        [.. inventory.Entries
            .Select(entry => items[entry.ItemId])
            .Where(item => item.Equipment is { } equipment && EquipmentFit.Fits(equipment.Kind, slot))];

    /// <summary>
    /// What the member could take from another member in the slot (decision 137): each other member's item there,
    /// active members first then reserve, in party order, that isn't the very item they already wear. One entry per
    /// wearer, so two members wearing the same sword give two entries.
    /// </summary>
    public static IReadOnlyList<WornItem> TakeableFor(PartyMember member, EquipmentSlot slot, PartyState party)
    {
        var takeable = new List<WornItem>();
        foreach (var wearer in party.ActiveMembers.Concat(party.ReserveMembers))
        {
            if (wearer != member && wearer.EquippedIn(slot) is { } piece && IsWorthTaking(piece, member, slot))
            {
                takeable.Add(new WornItem(wearer, piece));
            }
        }

        return takeable;
    }

    /// <summary>
    /// Moves the giver's item in the slot to the receiver (decision 137). The receiver's own item there goes back to the
    /// inventory, and the giver's slot is left empty.
    /// </summary>
    /// <returns>
    /// False if the giver is the receiver, the giver's slot is empty, the receiver already wears the same item, or the
    /// receiver's own item doesn't fit back in the inventory. Nothing changes when it returns false.
    /// </returns>
    public bool TryTakeFrom(PartyMember giver, PartyMember receiver, EquipmentSlot slot)
    {
        var piece = giver.EquippedIn(slot);
        if (giver == receiver || piece is null || !IsWorthTaking(piece, receiver, slot))
        {
            return false;
        }

        var replaced = receiver.EquippedIn(slot);
        if (replaced is not null && !inventory.HasRoomFor(replaced.Base))
        {
            return false;
        }

        if (replaced is not null)
        {
            inventory.Add(replaced.Base, 1);
        }

        giver.TakeOff(slot);
        receiver.Wear(slot, piece);
        return true;
    }

    /// <returns>False if the slot is empty or the item's inventory stack is full. Nothing changes when it returns false.</returns>
    public bool TryUnequip(PartyMember member, EquipmentSlot slot)
    {
        var worn = member.EquippedIn(slot);
        if (worn is null || !inventory.HasRoomFor(worn.Base))
        {
            return false;
        }

        inventory.Add(worn.Base, 1);
        member.TakeOff(slot);
        return true;
    }

    // Not a piece equal to the one the receiver already wears there, which taking would only shuffle. A worn piece is
    // always equipment, so nothing else needs checking (decision 165: anyone can wear it).
    private static bool IsWorthTaking(GearPiece piece, PartyMember receiver, EquipmentSlot slot) =>
        !piece.Equals(receiver.EquippedIn(slot));
}
