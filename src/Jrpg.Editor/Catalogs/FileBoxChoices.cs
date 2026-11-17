namespace Jrpg.Editor.Catalogs;

/// <summary>
/// What a box choosing one file of a folder offers, a map's track or battle background or an encounter's: the entry for
/// none first, then the folder's files. A file the record names that the folder no longer holds stays listed, so
/// opening the editor never changes the record (decisions 419, 906, 909).
/// </summary>
internal static class FileBoxChoices
{
    /// <param name="current">The file the record names; null for none.</param>
    /// <param name="none">The first entry, standing for no file of the record's own.</param>
    /// <param name="inFolder">The folder's files in the order the box lists them.</param>
    /// <returns>
    /// <paramref name="none"/>, <paramref name="current"/> when the folder lacks it, then the folder's files.
    /// </returns>
    public static IReadOnlyList<string> Of(string? current, string none, IReadOnlyList<string> inFolder) =>
        current is null || inFolder.Contains(current) ? [none, .. inFolder] : [none, current, .. inFolder];
}
