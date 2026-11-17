namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The first problem among the rows of a page that edits an art folder's records by file (the Effect strips, Character
/// sprites and Battle backgrounds pages): the chosen row's own, bare as the page shows that row, then the first other
/// row's, named by its file since the page doesn't show it.
/// </summary>
internal static class ArtImageRowProblem
{
    /// <param name="chosen">
    /// The row the page shows; null when no row is chosen, or when the chosen row's problem doesn't count yet (an
    /// unedited Character sprites setup).
    /// </param>
    /// <param name="rows">The rows whose problems count, in the list's order, the chosen one among them or not.</param>
    /// <param name="fileOf">A row's image file.</param>
    /// <param name="problemOf">A row's own problem, or null.</param>
    /// <returns>"problem", "file: problem", or null when no row has one.</returns>
    public static string? FirstOf<T>(T? chosen, IEnumerable<T> rows, Func<T, string> fileOf, Func<T, string?> problemOf)
        where T : class
    {
        if (chosen is not null && problemOf(chosen) is { } own)
        {
            return own;
        }

        return rows
            .Select(row => (File: fileOf(row), Problem: problemOf(row)))
            .Where(row => row.Problem is not null)
            .Select(row => $"{row.File}: {row.Problem}")
            .FirstOrDefault();
    }
}
