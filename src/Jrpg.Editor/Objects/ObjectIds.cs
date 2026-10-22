using System.Text.RegularExpressions;

namespace Jrpg.Editor.Objects;

/// <summary>What is wrong with a typed object id: ids are fixed once saved, as maps and saves refer to them.</summary>
internal static partial class ObjectIds
{
    /// <returns>Why the id can't be used, or null when it can.</returns>
    /// <param name="id">As typed.</param>
    /// <param name="takenIds">The object ids already in the catalog.</param>
    public static string? ProblemOf(string id, IReadOnlyCollection<string> takenIds)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return "The id must not be empty.";
        }

        if (!IdPattern().IsMatch(id))
        {
            return "The id may only use lowercase letters, digits and _ (like tree_4).";
        }

        return takenIds.Contains(id) ? $"The id {id} is taken." : null;
    }

    // Content ids are snake case, as every file in content/ uses (content rules).
    [GeneratedRegex("^[a-z0-9_]+$")]
    private static partial Regex IdPattern();
}
