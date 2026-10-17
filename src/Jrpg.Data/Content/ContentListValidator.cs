namespace Jrpg.Data.Content;

/// <summary>
/// The checks every content file holding a list of entries needs: no null entries, non-empty unique ids,
/// then each entry's own rules. Messages name the file, the entry and the field.
/// </summary>
internal sealed class ContentListValidator<T>(string fileName, string entryKind, Func<T, string> idOf)
    where T : class
{
    /// <param name="entries">The deserialized list.</param>
    /// <param name="findProblem">Returns what is wrong with one entry, or null if nothing is.</param>
    public void Validate(IReadOnlyList<T> entries, Func<T, string?> findProblem)
    {
        var seenIds = new HashSet<string>();
        for (var index = 0; index < entries.Count; index++)
        {
            // The serializer doesn't apply nullable annotations to collection elements, so a stray null gets here.
            var entry = entries[index];
            if (entry is null)
            {
                throw new ContentValidationException($"{fileName}: entry {index} is null.");
            }

            var id = idOf(entry);
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ContentValidationException($"{fileName}: entry {index} has an empty id.");
            }

            if (!seenIds.Add(id))
            {
                throw new ContentValidationException($"{fileName}: {entryKind} id '{id}' is used more than once.");
            }

            var problem = findProblem(entry);
            if (problem is not null)
            {
                throw new ContentValidationException($"{fileName}: {entryKind} '{id}': {problem}");
            }
        }
    }
}
