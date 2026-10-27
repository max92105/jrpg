using Jrpg.Core.Items;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Party;

/// <summary>
/// What a member wears in each of the nine slots (ADR 0028), and what it all adds up to: the bonuses of everything
/// worn, and the weapon's damage. Kept out of <see cref="PartyMember"/>, which only asks what is in a slot.
/// </summary>
internal sealed class WornEquipment
{
    private readonly Dictionary<EquipmentSlot, ItemDefinition> _items;

    public WornEquipment()
        : this([])
    {
    }

    private WornEquipment(Dictionary<EquipmentSlot, ItemDefinition> items)
    {
        _items = items;
    }

    /// <summary>What every worn item adds together, by stat kind.</summary>
    public StatBonuses Bonuses =>
        _items.Values.Aggregate(StatBonuses.None, (total, item) => total.Plus(item.Equipment?.Bonuses ?? StatBonuses.None));

    /// <summary>Each worn item's id by its slot, as a save keeps them; empty slots absent.</summary>
    public IReadOnlyDictionary<EquipmentSlot, string> ItemIds => _items.ToDictionary(entry => entry.Key, entry => entry.Value.Id);

    /// <summary>The worn weapon's damage; null without one, for the rules' unarmed damage (decision 330).</summary>
    public int? WeaponDamage => In(EquipmentSlot.Weapon)?.Equipment?.Damage;

    /// <returns>Null when the slot is empty.</returns>
    public ItemDefinition? In(EquipmentSlot slot) => _items.GetValueOrDefault(slot);

    /// <param name="slot">Where it goes.</param>
    /// <param name="item">What goes in the slot; null empties it.</param>
    public void Put(EquipmentSlot slot, ItemDefinition? item)
    {
        if (item is null)
        {
            _items.Remove(slot);
        }
        else
        {
            _items[slot] = item;
        }
    }

    /// <summary>A copy with the slot holding <paramref name="item"/> (or nothing), for a sheet preview.</summary>
    public WornEquipment With(EquipmentSlot slot, ItemDefinition? item)
    {
        var copy = new WornEquipment(new Dictionary<EquipmentSlot, ItemDefinition>(_items));
        copy.Put(slot, item);
        return copy;
    }
}
