namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The first thing wrong with a database window's list of records (ADR 0045): an id two records share, reported on the
/// later one so the first stays as it was, or a record's own problem, the record named by id, or by its position while
/// a new one has no id yet.
/// </summary>
internal static class RecordListProblem
{
    /// <param name="records">The records in the list's order.</param>
    /// <param name="idOf">A record's id as typed; empty while a new one has none.</param>
    /// <param name="problemOf">A record's own problem, or null.</param>
    /// <param name="noun">What the records are, capitalized: "Quest", "Shop".</param>
    /// <returns>
    /// "Shop x is listed twice.", "Shop x: problem", "Shop #3: problem", or null when nothing is wrong.
    /// </returns>
    public static string? FirstOf<T>(IReadOnlyList<T> records, Func<T, string> idOf, Func<T, string?> problemOf, string noun)
    {
        for (var index = 0; index < records.Count; index++)
        {
            var id = idOf(records[index]);
            if (records.Take(index).Any(earlier => idOf(earlier) == id))
            {
                return $"{noun} {id} is listed twice.";
            }

            if (problemOf(records[index]) is { } problem)
            {
                return $"{noun} {(id.Length > 0 ? id : $"#{index + 1}")}: {problem}";
            }
        }

        return null;
    }
}
