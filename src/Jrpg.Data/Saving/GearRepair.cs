using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Stats;

namespace Jrpg.Data.Saving;

/// <summary>
/// The gear half of <see cref="SaveGameRepair"/>: the pieces members wear, slot by slot, and the owned ones (ADR 0029).
/// A piece whose base no longer exists is dropped, one whose base is no longer equipment goes back to the stacks as one
/// copy, a worn one whose kind no longer fits its slot goes back to the owned gear, a unique base's piece follows its
/// record (ADR 0058), a saved Unique on a base no longer unique becomes plain, and an affix the pool no longer has is
/// dropped and one outside its range clamped into it; each is reported in the repair's changes.
/// </summary>
internal sealed class GearRepair
{
    private readonly ContentCatalog _catalog;
    private readonly List<string> _changes;
    private readonly List<SavedGear> _takenOff = [];
    private readonly List<string> _returnedItemIds = [];

    /// <param name="catalog">The content the save must fit.</param>
    /// <param name="changes">The repair's report, which this adds to.</param>
    public GearRepair(ContentCatalog catalog, List<string> changes)
    {
        _catalog = catalog;
        _changes = changes;
    }

    /// <summary>
    /// Pieces whose base is no longer equipment, worn or owned, one entry per copy in repair order: they join the
    /// stacks.
    /// </summary>
    public IReadOnlyList<string> ReturnedItemIds => _returnedItemIds;

    public Dictionary<EquipmentSlot, SavedGear> RepairWorn(PartyMemberProgress progress)
    {
        var owner = $"party member '{progress.Id}'";
        var repaired = new Dictionary<EquipmentSlot, SavedGear>();
        foreach (var (slot, saved) in progress.Worn)
        {
            if (!_catalog.Items.TryGetValue(saved.ItemId, out var item))
            {
                _changes.Add($"{owner}: dropped worn item '{saved.ItemId}', which no longer exists.");
            }
            else if (item.Equipment is { } equipment && EquipmentFit.Fits(equipment.Kind, slot))
            {
                repaired[slot] = RepairAffixes(saved, owner);
            }
            else
            {
                TakeOff(saved, item, owner);
                _changes.Add($"{owner}: '{saved.ItemId}' can no longer be worn as {slot}, so it went back to the inventory.");
            }
        }

        return repaired;
    }

    /// <summary>The owned pieces kept, in order, then the worn ones <see cref="RepairWorn"/> took off.</summary>
    /// <remarks>Call it after every member's <see cref="RepairWorn"/>.</remarks>
    public List<SavedGear> RepairOwned(IReadOnlyList<SavedGear> gear)
    {
        const string owner = "the inventory";
        var repaired = new List<SavedGear>();
        foreach (var saved in gear)
        {
            if (!_catalog.Items.TryGetValue(saved.ItemId, out var item))
            {
                _changes.Add($"{owner}: dropped gear '{saved.ItemId}', which no longer exists.");
            }
            else if (item.Equipment is null)
            {
                _returnedItemIds.Add(saved.ItemId);
                _changes.Add($"{owner}: '{saved.ItemId}' is no longer gear, so it went back to the stacks.");
            }
            else
            {
                repaired.Add(RepairAffixes(saved, owner));
            }
        }

        return [.. repaired, .. _takenOff];
    }

    /// <summary>
    /// What the repaired worn pieces add together, for the sheet that bounds the member's saved numbers.
    /// </summary>
    public StatBonuses BonusesOf(IReadOnlyDictionary<EquipmentSlot, SavedGear> worn) =>
        worn.Values.Aggregate(StatBonuses.None, (total, saved) => total.Plus(_catalog.PieceFrom(saved).Bonuses));

    /// <returns>Null without a worn weapon, for the rules' unarmed damage.</returns>
    public int? WeaponDamageOf(IReadOnlyDictionary<EquipmentSlot, SavedGear> worn) =>
        worn.TryGetValue(EquipmentSlot.Weapon, out var weapon) ? _catalog.Items[weapon.ItemId].Equipment?.Damage : null;

    // A piece whose base is no longer equipment goes back to the stacks; one that only no longer fits joins the gear.
    private void TakeOff(SavedGear saved, ItemDefinition item, string owner)
    {
        if (item.Equipment is null)
        {
            _returnedItemIds.Add(saved.ItemId);
        }
        else
        {
            _takenOff.Add(RepairAffixes(saved, owner));
        }
    }

    /// <summary>
    /// The piece as content now makes it, every change reported, never silent: a unique base gives its record's piece
    /// whatever the save held (decision 868); a saved Unique on a base no longer unique becomes plain; any other piece
    /// keeps only the affixes the pool still has, each value within its affix's current range (decision 360). Shared
    /// with the shops' stocks, which repair the same way.
    /// </summary>
    /// <param name="saved">The piece as the save keeps it; its base is in the catalog and is equipment.</param>
    /// <param name="owner">Who holds it, for the report.</param>
    internal SavedGear RepairAffixes(SavedGear saved, string owner)
    {
        var item = _catalog.Items[saved.ItemId];
        if (item.IsUnique)
        {
            return FollowRecord(saved, item, owner);
        }

        // Made plain before the worn bonuses' sum, which would otherwise count the saved affixes of a Unique rarity on
        // an ordinary base.
        if (saved.Rarity == Rarity.Unique)
        {
            _changes.Add($"{owner}: '{saved.ItemId}' is no longer a unique, so it became a plain piece.");
            return saved with { Rarity = Rarity.Common, Affixes = [] };
        }

        return ClampAffixes(saved, owner);
    }

    // The record is the truth (decision 868), and its values may lie outside their affixes' roll ranges (decision 866),
    // so they are never clamped.
    private SavedGear FollowRecord(SavedGear saved, ItemDefinition item, string owner)
    {
        var record = GearPiece.Unique(item).ToSaved();
        if (saved != record)
        {
            _changes.Add($"{owner}: unique '{saved.ItemId}' follows its record now.");
        }

        return record;
    }

    private SavedGear ClampAffixes(SavedGear saved, string owner)
    {
        var repaired = new List<SavedAffix>();
        foreach (var affix in saved.Affixes)
        {
            if (!_catalog.Affixes.TryGetValue(affix.AffixId, out var definition))
            {
                _changes.Add($"{owner}: dropped affix '{affix.AffixId}' from '{saved.ItemId}', which no longer exists.");
                continue;
            }

            var value = Math.Clamp(affix.Value, definition.Min, definition.Max);
            if (value != affix.Value)
            {
                _changes.Add($"{owner}: affix '{affix.AffixId}' on '{saved.ItemId}' brought within {definition.Min}..{definition.Max} (was {affix.Value}).");
            }

            repaired.Add(affix with { Value = value });
        }

        return saved with { Affixes = repaired };
    }
}
