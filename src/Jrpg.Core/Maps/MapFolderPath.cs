namespace Jrpg.Core.Maps;

/// <summary>
/// The folder a map is listed under in the editor (decision 532): names joined by <see cref="Separator"/>, nested ("Act
/// 1/Village"). The game never reads it; the rule lives here so content and the editor agree on it.
/// </summary>
public static class MapFolderPath
{
    public const char Separator = '/';

    private const string Rule = "A folder path is names joined by '/', each with text, no space at either end and no backslash.";

    /// <summary>Why a path can't be a folder; null when it can. An empty path is refused: none is null.</summary>
    public static string? ProblemOf(string path) =>
        NamesOf(path).All(name => name.Length > 0 && name.Trim() == name && !name.Contains('\\')) ? null : Rule;

    /// <summary>The folder names from the top down.</summary>
    public static IReadOnlyList<string> NamesOf(string path) => path.Split(Separator);

    /// <summary>
    /// Whether a path is the folder itself or under it, by whole names: "Act 1" holds "Act 1/Village", not "Act 10".
    /// </summary>
    public static bool IsWithin(string path, string folder) =>
        path == folder || path.StartsWith(folder + Separator, StringComparison.Ordinal);

    /// <summary>
    /// The path once the folder it is within is renamed: "Act 1/Village" under "Act 1" renamed "Chapter 1" is "Chapter
    /// 1/Village"; the folder itself becomes the new path.
    /// </summary>
    /// <param name="path">A path within <paramref name="oldFolder"/>.</param>
    /// <param name="oldFolder">The folder renamed.</param>
    /// <param name="newFolder">Its new path.</param>
    public static string Moved(string path, string oldFolder, string newFolder) => newFolder + path[oldFolder.Length..];
}
