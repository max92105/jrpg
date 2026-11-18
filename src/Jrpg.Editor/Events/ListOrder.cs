using System.Collections.ObjectModel;

namespace Jrpg.Editor.Events;

/// <summary>
/// Adding, reordering and removing in the editor's lists, as their Add, Up, Down and Remove buttons do, numbering their
/// rows after each change, and writing a list back over the record it came from.
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

    /// <summary>
    /// True when the chosen row can move up: one is chosen and it isn't first. Its list re-greys Up and Down on every
    /// change of the list, since a move changes where the chosen row stands.
    /// </summary>
    public static bool CanMoveUp<T>(ObservableCollection<T> items, T? chosen)
        where T : class =>
        chosen is not null && items.IndexOf(chosen) > 0;

    /// <summary>True when the chosen row can move down: one is chosen and it isn't last.</summary>
    public static bool CanMoveDown<T>(ObservableCollection<T> items, T? chosen)
        where T : class =>
        chosen is not null && items.IndexOf(chosen) < items.Count - 1;

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

    /// <summary>
    /// The record's own list while the list as typed equals it in order, else the typed one: a record compares its
    /// lists by reference, so an untouched record still equals the one it came from (decision 1014).
    /// </summary>
    public static IReadOnlyList<T> KeptIfEqual<T>(IReadOnlyList<T> typed, IReadOnlyList<T> source) =>
        typed.SequenceEqual(source) ? source : typed;
}
