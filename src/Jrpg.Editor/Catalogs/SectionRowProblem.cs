namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The first row of a page's section that can't be built (a skill's cost changes, needs and grants, a status's parts),
/// named by its list and its place as the page shows it.
/// </summary>
internal static class SectionRowProblem
{
    /// <param name="list">The list's name on the page: "Changes", "Statuses", "Parts".</param>
    /// <param name="rows">The rows in the page's order.</param>
    /// <param name="problemOf">A row's own problem, or null.</param>
    /// <returns>"Changes row 2: Amount must be a whole number (was 'x')."; null when every row can be built.</returns>
    public static string? FirstOf<T>(string list, IReadOnlyList<T> rows, Func<T, string?> problemOf)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            if (problemOf(rows[index]) is { } problem)
            {
                return $"{list} row {index + 1}: {problem}";
            }
        }

        return null;
    }
}
