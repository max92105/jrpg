namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Uids for newly placed objects: <c>{objectId}_{n}</c> with n one above the highest on the map, such as <c>chest_3</c>
/// next to <c>chest_2</c>, so they read well in content and need no randomness (plan decision 8). A number freed by a
/// deleted object isn't reused, so a new object doesn't inherit an old save's state for it (decision 87). Uids already
/// on maps are never changed, since saves remember objects by them.
/// </summary>
internal static class PlacedObjectUids
{
    /// <param name="objectId">The placed object's definition id.</param>
    /// <param name="taken">The uids already on the map.</param>
    public static string Next(string objectId, IReadOnlyCollection<string> taken)
    {
        var prefix = $"{objectId}_";
        var highest = NumberedIds.HighestNumber(prefix, taken) ?? 0;
        return $"{prefix}{checked(highest + 1)}";
    }
}
