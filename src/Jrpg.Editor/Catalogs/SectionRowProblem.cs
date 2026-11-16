namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The first row of a page's section that can't be built (a skill's cost changes, needs and grants, a status's parts,
/// an enemy's rules), named by its list and its place as the page shows it, or by Data's field where the page numbers
/// its rows as Data does.
/// </summary>
internal static class SectionRowProblem
{
    /// <param name="list">The list's name on the page: "Changes", "Statuses", "Parts".</param>
    /// <param name="rows">The rows in the page's order.</param>
    /// <param name="problemOf">A row's own problem, or null.</param>
    /// <returns>"Changes row 2: Amount must be a whole number (was 'x')."; null when every row can be built.</returns>
    public static string? FirstOf<T>(string list, IReadOnlyList<T> rows, Func<T, string?> problemOf) =>
        First(rows, problemOf) is { } found ? $"{list} row {found.Index + 1}: {found.Problem}" : null;

    /// <param name="field">Data's name for the list: "tactics", "phases[1].tactics".</param>
    /// <param name="rows">The rows in the file's order.</param>
    /// <param name="problemOf">A row's own problem, or null.</param>
    /// <returns>
    /// "phases[0].tactics[2]: Weight must be a whole number (was 'x')."; null when every row can be built.
    /// </returns>
    public static string? FirstInField<T>(string field, IReadOnlyList<T> rows, Func<T, string?> problemOf) =>
        First(rows, problemOf) is { } found ? $"{field}[{found.Index}]: {found.Problem}" : null;

    private static (int Index, string Problem)? First<T>(IReadOnlyList<T> rows, Func<T, string?> problemOf)
    {
        for (var index = 0; index < rows.Count; index++)
        {
            if (problemOf(rows[index]) is { } problem)
            {
                return (index, problem);
            }
        }

        return null;
    }
}
