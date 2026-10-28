using Jrpg.Core.Items;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>
/// What a member wears in each of the nine slots (ADR 0028), and what it all adds up to: the bonuses of everything
/// worn, and the weapon's damage. Kept out of <see cref="PartyMember"/>, which only asks what is in a slot.
/// </summary>
internal sealed class WornEquipment
{
    private readonly Dictionary<EquipmentSlot, GearPiece> _pieces;

    public WornEquipment()
        : this([])
    {
    }

    private WornEquipment(Dictionary<EquipmentSlot, GearPiece> pieces)
    {
        _pieces = pieces;
    }

    /// <summary>What every worn piece adds together, by stat kind: base bonuses and affixes.</summary>
    public StatBonuses Bonuses =>
        _pieces.Values.Aggregate(StatBonuses.None, (total, piece) => total.Plus(piece.Bonuses));

    /// <summary>Each worn item's id by its slot, as a save keeps them; empty slots absent.</summary>
    public IReadOnlyDictionary<EquipmentSlot, string> ItemIds => _pieces.ToDictionary(entry => entry.Key, entry => entry.Value.Base.Id);

    /// <summary>The worn weapon's damage; null without one, for the rules' unarmed damage (decision 330).</summary>
    public int? WeaponDamage => In(EquipmentSlot.Weapon)?.Damage;

    /// <returns>Null when the slot is empty.</returns>
    public GearPiece? In(EquipmentSlot slot) => _pieces.GetValueOrDefault(slot);

    /// <param name="slot">Where it goes.</param>
    /// <param name="piece">What goes in the slot; null empties it.</param>
    public void Put(EquipmentSlot slot, GearPiece? piece)
    {
        if (piece is null)
        {
            _pieces.Remove(slot);
        }
        else
        {
            _pieces[slot] = piece;
        }
    }

    /// <summary>A copy with the slot holding <paramref name="piece"/> (or nothing), for a sheet preview.</summary>
    public WornEquipment With(EquipmentSlot slot, GearPiece? piece)
    {
        var copy = new WornEquipment(new Dictionary<EquipmentSlot, GearPiece>(_pieces));
        copy.Put(slot, piece);
        return copy;
    }
}
