using System.Collections.Specialized;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// Keeps a list view model watching exactly the rows its collection holds, for the windows whose problem line relays
/// each row's changes (the quests' stages, the drop tables' entries, the cinematics' stills).
/// </summary>
internal static class ListItemWatch
{
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
