namespace Jrpg.Core.Items;

/// <summary>
/// What the party owns, worn gear aside (ADR 0003): stacks of consumables in pickup order, and gear as one piece per
/// row in pickup order, with no cap (ADR 0029, decision 345).
/// </summary>
public sealed class Inventory
{
    private readonly List<InventoryEntry> _entries = [];
    private readonly List<GearPiece> _gear = [];

    /// <summary>The stacks: everything owned but gear.</summary>
    public IReadOnlyList<InventoryEntry> Entries => _entries;

    /// <summary>Owned gear, one piece per row, in pickup order.</summary>
    public IReadOnlyList<GearPiece> Gear => _gear;

    /// <summary>Stacks the item, or adds equipment as that many plain pieces (decision 344).</summary>
    /// <returns>
    /// How many copies didn't fit because the stack reached <see cref="ItemDefinition.MaxStack"/>; always 0 for
    /// equipment, which has no cap.
    /// </returns>
    public int Add(ItemDefinition item, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        if (item.Equipment is not null)
        {
            _gear.AddRange(Enumerable.Repeat(GearPiece.Plain(item), quantity));
            return 0;
        }

        var index = IndexOf(item.Id);
        var owned = index < 0 ? 0 : _entries[index].Quantity;
        var added = Math.Min(quantity, item.MaxStack - owned);
        if (added <= 0)
        {
            return quantity;
        }

        var entry = new InventoryEntry(item.Id, owned + added);
        if (index < 0)
        {
            _entries.Add(entry);
        }
        else
        {
            _entries[index] = entry;
        }

        return quantity - added;
    }

    public void AddGear(GearPiece piece) => _gear.Add(piece);

    /// <summary>
    /// Removes the first owned piece equal to <paramref name="piece"/>: the same base, rarity and affixes.
    /// </summary>
    /// <returns>False when no owned piece is equal to it; nothing changes then.</returns>
    public bool TryRemoveGear(GearPiece piece)
    {
        var index = _gear.IndexOf(piece);
        if (index < 0)
        {
            return false;
        }

        _gear.RemoveAt(index);
        return true;
    }

    /// <summary>How many owned pieces have <paramref name="itemId"/> as their base, whatever they rolled.</summary>
    public int GearCountOf(string itemId) => _gear.Count(piece => piece.Base.Id == itemId);

    /// <summary>Takes copies off a stack; gear is never in one (see <see cref="TryRemoveGear"/>).</summary>
    public bool TryRemove(string itemId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var index = IndexOf(itemId);
        if (index < 0 || _entries[index].Quantity < quantity)
        {
            return false;
        }

        var remaining = _entries[index].Quantity - quantity;
        if (remaining == 0)
        {
            _entries.RemoveAt(index);
        }
        else
        {
            _entries[index] = new InventoryEntry(itemId, remaining);
        }

        return true;
    }

    /// <summary>A stack's size; 0 for gear, which <see cref="GearCountOf"/> counts.</summary>
    public int QuantityOf(string itemId)
    {
        var index = IndexOf(itemId);
        return index < 0 ? 0 : _entries[index].Quantity;
    }

    /// <summary>Whether the item's stack can take one more; gear is never in a stack, so always true for it.</summary>
    public bool HasRoomFor(ItemDefinition item) => QuantityOf(item.Id) < item.MaxStack;

    private int IndexOf(string itemId) => _entries.FindIndex(entry => entry.ItemId == itemId);
}
