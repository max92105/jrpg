using System.Collections.ObjectModel;

namespace Jrpg.Editor.Events;

/// <summary>
/// Adding, reordering and removing in the editor's lists, as their Add, Up, Down and Remove buttons do, and numbering
/// their rows after each change.
/// </summary>
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

    /// <summary>Puts an item right after the chosen one, or last when none is chosen.</summary>
    public static void InsertAfter<T>(ObservableCollection<T> items, T item, T? chosen)
        where T : class =>
        items.Insert(chosen is null ? items.Count : items.IndexOf(chosen) + 1, item);

    /// <summary>Gives each item its place from 0, which the caller shows as it numbers its rows.</summary>
    public static void Renumber<T>(IReadOnlyList<T> items, Action<T, int> setPlace)
    {
        for (var index = 0; index < items.Count; index++)
        {
            setPlace(items[index], index);
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
