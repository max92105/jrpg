using Jrpg.Core.Items;

namespace Jrpg.Core.Party;

/// <summary>
/// Moves gear pieces between the inventory and party members. A worn piece is not in the inventory (ADR 0003), so
/// equipping takes it out and whatever it replaces goes back in, keeping its rolls (ADR 0029).
/// </summary>
public sealed class PartyEquipment(Inventory inventory)
{
    /// <summary>Equips the piece in the first slot its kind fits: a ring in the first ring slot.</summary>
    /// <returns>As <see cref="TryEquip(PartyMember, GearPiece, EquipmentSlot)"/>.</returns>
    public bool TryEquip(PartyMember member, GearPiece piece) =>
        TryEquip(member, piece, EquipmentFit.SlotsFor(piece.Kind)[0]);

    /// <returns>
    /// True if the member wears the piece in the slot afterwards; true without changes if they already wore an equal
    /// piece there. False if the piece's kind doesn't fit the slot (ADR 0028) or no equal piece is owned. Nothing
    /// changes when it returns false. Gear has no cap, so what it replaces always goes back (decision 345).
    /// </returns>
    public bool TryEquip(PartyMember member, GearPiece piece, EquipmentSlot slot)
    {
        if (!EquipmentFit.Fits(piece.Kind, slot))
        {
            return false;
        }

        var replaced = member.EquippedIn(slot);
        if (piece.Equals(replaced))
        {
            return true;
        }

        if (!inventory.TryRemoveGear(piece))
        {
            return false;
        }

        if (replaced is not null)
        {
            inventory.AddGear(replaced);
        }

        member.Wear(slot, piece);
        return true;
    }

    /// <summary>What any member could put on in the slot: each owned piece that fits it, in pickup order.</summary>
    public IReadOnlyList<GearPiece> OwnedFor(EquipmentSlot slot) =>
        [.. inventory.Gear.Where(piece => EquipmentFit.Fits(piece.Kind, slot))];

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
    /// False if the giver is the receiver, the giver's slot is empty, or the receiver already wears an equal piece.
    /// Nothing changes when it returns false.
    /// </returns>
    public bool TryTakeFrom(PartyMember giver, PartyMember receiver, EquipmentSlot slot)
    {
        var piece = giver.EquippedIn(slot);
        if (giver == receiver || piece is null || !IsWorthTaking(piece, receiver, slot))
        {
            return false;
        }

        if (receiver.EquippedIn(slot) is { } replaced)
        {
            inventory.AddGear(replaced);
        }

        giver.TakeOff(slot);
        receiver.Wear(slot, piece);
        return true;
    }

    /// <returns>False if the slot is empty; nothing changes then. Gear has no cap (decision 345).</returns>
    public bool TryUnequip(PartyMember member, EquipmentSlot slot)
    {
        var worn = member.EquippedIn(slot);
        if (worn is null)
        {
            return false;
        }

        inventory.AddGear(worn);
        member.TakeOff(slot);
        return true;
    }

    // Not a piece equal to the one the receiver already wears there, which taking would only shuffle. A worn piece is
    // always equipment, so nothing else needs checking (decision 165: anyone can wear it).
    private static bool IsWorthTaking(GearPiece piece, PartyMember receiver, EquipmentSlot slot) =>
        !piece.Equals(receiver.EquippedIn(slot));
}
