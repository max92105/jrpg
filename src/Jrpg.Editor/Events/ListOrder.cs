using System.Collections.ObjectModel;

namespace Jrpg.Editor.Events;

/// <summary>Reordering and removing in the event dialog's lists, as their Up, Down and Remove buttons do.</summary>
internal static class ListOrder
{
    /// <summary>Moves an item up (negative) or down; it stays put at either end.</summary>
    public static void MoveBy<T>(ObservableCollection<T> items, T item, int offset)
    {
        var from = items.IndexOf(item);
        var to = Math.Clamp(from + offset, 0, items.Count - 1);
        if (from >= 0 && to != from)
        {
            items.Move(from, to);
        }
    }

    /// <summary>Removes an item.</summary>
    /// <returns>The item to select next: the one that took its place, else the one before, or null when none is left.</returns>
    public static T? Remove<T>(ObservableCollection<T> items, T item)
        where T : class
    {
        var index = items.IndexOf(item);
        items.RemoveAt(index);
        return items.Count == 0 ? null : items[Math.Min(index, items.Count - 1)];
    }
}
