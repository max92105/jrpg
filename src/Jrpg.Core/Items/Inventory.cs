namespace Jrpg.Core.Items;

/// <summary>Owned item quantities in pickup order. Worn equipment is not in here.</summary>
public sealed class Inventory
{
    private readonly List<InventoryEntry> _entries = [];

    public IReadOnlyList<InventoryEntry> Entries => _entries;

    /// <returns>How many copies didn't fit because the stack reached <see cref="ItemDefinition.MaxStack"/>.</returns>
    public int Add(ItemDefinition item, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

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

    public int QuantityOf(string itemId)
    {
        var index = IndexOf(itemId);
        return index < 0 ? 0 : _entries[index].Quantity;
    }

    private int IndexOf(string itemId) => _entries.FindIndex(entry => entry.ItemId == itemId);
}
