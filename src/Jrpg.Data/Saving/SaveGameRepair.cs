using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Saving;
using Jrpg.Core.Stats;

namespace Jrpg.Data.Saving;

/// <summary>
/// Makes a save fit the current content (developer decision 40): what no longer exists is dropped, what no longer fits
/// the rules is brought back into range, and the rest is kept. Placeholders, like the rest of part 2's rules: an item
/// that can no longer be worn goes back to the inventory, and a save on a removed map starts at the new game's start.
/// </summary>
internal sealed class SaveGameRepair
{
    private readonly ContentCatalog _catalog;
    private readonly List<string> _changes = [];
    private readonly List<string> _returnedItemIds = [];

    private SaveGameRepair(ContentCatalog catalog)
    {
        _catalog = catalog;
    }

    /// <exception cref="UnreadableSaveException">None of the saved active members exists anymore, so there's nobody to play.</exception>
    public static RepairedSave Repair(string slotName, SaveGame save, ContentCatalog catalog)
    {
        var repair = new SaveGameRepair(catalog);
        var members = repair.RepairMembers(save.Members);
        var activeMemberIds = repair.RepairActiveOrder(save.ActiveMemberIds, members);
        if (activeMemberIds.Count == 0)
        {
            throw new UnreadableSaveException(slotName, "none of its active party members exist anymore.");
        }

        var repaired = save with
        {
            Members = members,
            ActiveMemberIds = activeMemberIds,
            Inventory = repair.RepairInventory(save.Inventory),
            CompletedEvents = WorldStateRepair.KeepKnownEvents(save.CompletedEvents, catalog, repair._changes),
            ObjectSwaps = WorldStateRepair.KeepKnownSwaps(save.ObjectSwaps, catalog, repair._changes),
            Location = repair.RepairLocation(save.Location),
        };
        return new RepairedSave(repaired, repair._changes);
    }

    private List<PartyMemberProgress> RepairMembers(IReadOnlyList<PartyMemberProgress> members)
    {
        var repaired = new List<PartyMemberProgress>();
        foreach (var progress in members)
        {
            if (!_catalog.PartyMembers.TryGetValue(progress.Id, out var definition))
            {
                _changes.Add($"dropped party member '{progress.Id}', who no longer exists.");
            }
            else if (repaired.Any(member => member.Id == progress.Id))
            {
                _changes.Add($"dropped a second copy of party member '{progress.Id}'.");
            }
            else
            {
                repaired.Add(RepairMember(progress, definition));
            }
        }

        return repaired;
    }

    private PartyMemberProgress RepairMember(PartyMemberProgress progress, PartyMemberDefinition definition)
    {
        var (level, experience) = RepairLevel(progress);
        var characterClass = _catalog.ClassOf(definition);
        var atLevel = ClassGrowth.StatsAt(characterClass, level);
        var maxHp = StatFormula.MaxHp(atLevel.MaxHp, atLevel.Primaries.Constitution, _catalog.Rules.Stats);
        var repaired = progress with
        {
            Level = level,
            Experience = experience,
            Hp = Math.Clamp(progress.Hp, 0, maxHp),
            WeaponId = RepairWornItem(progress.WeaponId, EquipmentSlot.Weapon, definition),
            ArmorId = RepairWornItem(progress.ArmorId, EquipmentSlot.Armor, definition),
            LastingAmounts = RepairLastingAmounts(progress, characterClass),
        };
        if (repaired.Hp != progress.Hp)
        {
            _changes.Add($"party member '{progress.Id}': HP {progress.Hp} brought within 0..{maxHp}.");
        }

        return repaired;
    }

    // Only the class's lasting resources, each within 0..max; one the save doesn't hold starts fresh on restore.
    private Dictionary<string, int> RepairLastingAmounts(PartyMemberProgress progress, ClassDefinition characterClass)
    {
        var repaired = new Dictionary<string, int>();
        foreach (var (resourceId, amount) in progress.LastingAmounts)
        {
            var resource = characterClass.Resources.FirstOrDefault(resource => resource.Id == resourceId && resource.IsLasting);
            if (resource is null)
            {
                _changes.Add($"party member '{progress.Id}': dropped lasting resource '{resourceId}', which the class no longer has as lasting.");
                continue;
            }

            repaired[resourceId] = Math.Clamp(amount, 0, resource.Max);
            if (repaired[resourceId] != amount)
            {
                _changes.Add($"party member '{progress.Id}': {resourceId} {amount} brought within 0..{resource.Max}.");
            }
        }

        return repaired;
    }

    // Within the experience table, with experience below the next threshold so the first gain never steps backwards.
    private (int Level, int Experience) RepairLevel(PartyMemberProgress progress)
    {
        var table = _catalog.Rules.Experience;
        var level = Math.Clamp(progress.Level, 1, table.MaxLevel);
        var experience = table.IsMaxLevel(level)
            ? 0
            : Math.Clamp(progress.Experience, 0, table.ExperienceToReachNext(level) - 1);
        if ((level, experience) != (progress.Level, progress.Experience))
        {
            _changes.Add($"party member '{progress.Id}': level {progress.Level} with {progress.Experience} experience became level {level} with {experience}.");
        }

        return (level, experience);
    }

    private string? RepairWornItem(string? itemId, EquipmentSlot slot, PartyMemberDefinition definition)
    {
        if (itemId is null)
        {
            return null;
        }

        if (!_catalog.Items.TryGetValue(itemId, out var item))
        {
            _changes.Add($"party member '{definition.Id}': dropped worn item '{itemId}', which no longer exists.");
            return null;
        }

        if (item.Equipment is not { } equipment || equipment.Slot != slot)
        {
            _returnedItemIds.Add(itemId);
            _changes.Add($"party member '{definition.Id}': '{itemId}' can no longer be worn as {slot}, so it went back to the inventory.");
            return null;
        }

        return itemId;
    }

    private List<string> RepairActiveOrder(IReadOnlyList<string> activeIds, List<PartyMemberProgress> members)
    {
        var known = new List<string>();
        foreach (var id in activeIds)
        {
            if (known.Contains(id))
            {
                _changes.Add($"'{id}' was listed twice in the active party; kept once.");
            }
            else if (members.Any(member => member.Id == id))
            {
                known.Add(id);
            }
            else if (_catalog.PartyMembers.ContainsKey(id))
            {
                // Members who no longer exist are already reported by RepairMembers.
                _changes.Add($"'{id}' was in the active party but not among the recruited members; removed.");
            }
        }

        var maxActive = _catalog.Rules.Party.MaxActiveMembers;
        if (known.Count > maxActive)
        {
            _changes.Add($"moved {string.Join(", ", known.Skip(maxActive).Select(id => $"'{id}'"))} to the reserve; the active party holds {maxActive}.");
            known = [.. known.Take(maxActive)];
        }

        return known;
    }

    // Worn items that went back are added last; duplicate entries merge, then each stack is kept within its MaxStack.
    private List<InventoryEntry> RepairInventory(IReadOnlyList<InventoryEntry> entries)
    {
        var quantities = new Dictionary<string, int>();
        var order = new List<string>();
        foreach (var (itemId, quantity) in entries.Select(entry => (entry.ItemId, entry.Quantity)).Concat(_returnedItemIds.Select(id => (id, 1))))
        {
            if (!_catalog.Items.ContainsKey(itemId) || quantity < 1)
            {
                _changes.Add($"dropped {quantity} x '{itemId}' from the inventory; the item no longer exists or the quantity isn't valid.");
                continue;
            }

            if (quantities.TryAdd(itemId, quantity))
            {
                order.Add(itemId);
            }
            else
            {
                // Saturates instead of overflowing; MaxStack is applied right after.
                quantities[itemId] = (int)Math.Min((long)quantities[itemId] + quantity, int.MaxValue);
            }
        }

        return [.. order.Select(itemId => new InventoryEntry(itemId, ClampToMaxStack(itemId, quantities[itemId])))];
    }

    private int ClampToMaxStack(string itemId, int quantity)
    {
        var maxStack = _catalog.Items[itemId].MaxStack;
        if (quantity <= maxStack)
        {
            return quantity;
        }

        _changes.Add($"kept {maxStack} x '{itemId}' of {quantity}; the stack holds {maxStack}.");
        return maxStack;
    }

    // A position outside a still existing map is left to WarpArrival, which finds the nearest walkable cell on loading.
    private MapLocation RepairLocation(MapLocation location)
    {
        if (_catalog.Maps.ContainsKey(location.MapId))
        {
            return location;
        }

        _changes.Add($"map '{location.MapId}' no longer exists, so the party starts at the new game's start.");
        return _catalog.NewGame.StartLocation;
    }
}
