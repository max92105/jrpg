using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Data.Saving;

/// <summary>
/// The gear half of <see cref="SaveGameRepair"/>: what each member wears, slot by slot. A worn item that no longer
/// exists is dropped, one that no longer fits its slot goes back to the inventory; both are reported in the repair's
/// changes.
/// </summary>
internal sealed class GearRepair
{
    private readonly ContentCatalog _catalog;
    private readonly List<string> _changes;
    private readonly List<string> _returnedItemIds = [];

    /// <param name="catalog">The content the save must fit.</param>
    /// <param name="changes">The repair's report, which this adds to.</param>
    public GearRepair(ContentCatalog catalog, List<string> changes)
    {
        _catalog = catalog;
        _changes = changes;
    }

    /// <summary>
    /// The items taken off because they no longer fit their slot, one entry per copy, in repair order.
    /// </summary>
    public IReadOnlyList<string> ReturnedItemIds => _returnedItemIds;

    public Dictionary<EquipmentSlot, string> RepairWorn(PartyMemberProgress progress)
    {
        var repaired = new Dictionary<EquipmentSlot, string>();
        foreach (var (slot, itemId) in progress.Worn)
        {
            if (RepairWornItem(itemId, slot, progress.Id) is { } kept)
            {
                repaired[slot] = kept;
            }
        }

        return repaired;
    }

    /// <summary>
    /// What the repaired worn items add together, for the sheet that bounds the member's saved numbers.
    /// </summary>
    public StatBonuses BonusesOf(IReadOnlyDictionary<EquipmentSlot, string> worn) =>
        worn.Values.Aggregate(StatBonuses.None, (total, itemId) => total.Plus(_catalog.Items[itemId].Equipment?.Bonuses ?? StatBonuses.None));

    /// <returns>Null without a worn weapon, for the rules' unarmed damage.</returns>
    public int? WeaponDamageOf(IReadOnlyDictionary<EquipmentSlot, string> worn) =>
        worn.TryGetValue(EquipmentSlot.Weapon, out var weaponId) ? _catalog.Items[weaponId].Equipment?.Damage : null;

    private string? RepairWornItem(string itemId, EquipmentSlot slot, string memberId)
    {
        if (!_catalog.Items.TryGetValue(itemId, out var item))
        {
            _changes.Add($"party member '{memberId}': dropped worn item '{itemId}', which no longer exists.");
            return null;
        }

        if (item.Equipment is not { } equipment || !EquipmentFit.Fits(equipment.Kind, slot))
        {
            _returnedItemIds.Add(itemId);
            _changes.Add($"party member '{memberId}': '{itemId}' can no longer be worn as {slot}, so it went back to the inventory.");
            return null;
        }

        return itemId;
    }
}
