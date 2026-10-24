namespace Jrpg.Data.Content;

/// <summary>
/// File names that content must match exactly, case included: Windows finds "goblin.png" for "Goblin.png", other
/// systems don't. Shared by the art and audio checks.
/// </summary>
internal static class ExactFileName
{
    /// <returns>
    /// A problem naming the file as it really is when the folder holds it only under another case; null when it is
    /// there exactly, or not there at all, which the caller reports as missing.
    /// </returns>
    public static string? FindCaseMismatch(string root, string folder, string file)
    {
        var directory = Path.Combine(root, folder);
        if (!Directory.Exists(directory))
        {
            return null;
        }

        var names = Directory.EnumerateFiles(directory).Select(Path.GetFileName).ToList();
        if (names.Contains(file, StringComparer.Ordinal))
        {
            return null;
        }

        var actual = names.FirstOrDefault(name => string.Equals(name, file, StringComparison.OrdinalIgnoreCase));
        return actual is null ? null : $"{ShownPath(folder, file)}: the file is named {actual}; names must match exactly.";
    }

    /// <summary>The file as a problem names it: under its folder, or alone when it sits at the root.</summary>
    public static string ShownPath(string folder, string file) => folder.Length == 0 ? file : $"{folder}/{file}";
}
