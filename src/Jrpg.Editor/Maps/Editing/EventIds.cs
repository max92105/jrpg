using System.Globalization;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Ids for new object events: <c>{uid}_{n}</c>, as the content's existing ones, with n one above the highest index the
/// object has used, so reordering events keeps their ids and only deleting the object's last event can free its id (plan
/// decision 8 and Risks). Saves remember "once" events by id.
/// </summary>
internal static class EventIds
{
    /// <param name="uid">The placed object's uid.</param>
    /// <param name="usedIds">The ids its events have, or had while the events were being edited.</param>
    public static string Next(string uid, IEnumerable<string> usedIds)
    {
        var prefix = $"{uid}_";
        var highest = usedIds
            .Where(id => id.StartsWith(prefix, StringComparison.Ordinal))
            .Select(id => int.TryParse(id.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var index) ? index : -1)
            .DefaultIfEmpty(-1)
            .Max();
        return $"{prefix}{highest + 1}";
    }
}
