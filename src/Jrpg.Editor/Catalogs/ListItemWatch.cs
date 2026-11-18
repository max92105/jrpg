using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// Keeps a list view model watching exactly the rows its collection holds, for the pages and windows whose problem line
/// relays each row's changes (the quests' stages, the drop tables' entries, the cinematics' stills, a class's skills
/// and its resources).
/// </summary>
internal static class ListItemWatch
{
    /// <summary>
    /// Watches every row the list holds now and every row it gains, stops watching the rows it loses, and runs
    /// <paramref name="listChanged"/> after each change of the list, where its owner re-greys its moves and raises its
    /// problem.
    /// </summary>
    public static void WatchRowsOf<T>(ObservableCollection<T> rows, Action<T> watch, Action<T> unwatch, Action listChanged)
    {
        foreach (var row in rows)
        {
            watch(row);
        }

        rows.CollectionChanged += (_, change) =>
        {
            Follow(change, watch, unwatch);
            listChanged();
        };
    }

    /// <summary>
    /// Stops watching the rows the change took out, then watches the ones it put in. In that order a Move, which lists
    /// its row as both removed and added, leaves the row watched once.
    /// </summary>
    public static void Follow<T>(NotifyCollectionChangedEventArgs change, Action<T> watch, Action<T> unwatch)
    {
        foreach (var row in change.OldItems?.OfType<T>() ?? [])
        {
            unwatch(row);
        }

        foreach (var row in change.NewItems?.OfType<T>() ?? [])
        {
            watch(row);
        }
    }
}
