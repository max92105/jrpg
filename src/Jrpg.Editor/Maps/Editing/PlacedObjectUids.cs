namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Uids for newly placed objects: <c>{objectId}_{n}</c> with the lowest n free on the map, such as <c>chest_2</c>, so
/// they read well in content and need no randomness (plan decision 8). Uids already on maps are never changed, since
/// saves remember objects by them.
/// </summary>
internal static class PlacedObjectUids
{
    /// <param name="objectId">The placed object's definition id.</param>
    /// <param name="taken">The uids already on the map.</param>
    public static string Next(string objectId, IReadOnlyCollection<string> taken)
    {
        var suffix = 1;
        while (taken.Contains($"{objectId}_{suffix}"))
        {
            suffix++;
        }

        return $"{objectId}_{suffix}";
    }
}
