using System.Text;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Ids for new maps, made from the name typed for them: snake_case like the content's other ids, and unique. An id is
/// fixed once the map exists (question 70), since saves and warps refer to it; the display name can change later.
/// </summary>
internal static class MapIds
{
    // When the name has no letters or digits at all.
    private const string Fallback = "map";

    /// <param name="name">The map's display name, such as "Old Mill".</param>
    /// <param name="taken">The ids already used; compared without case, since map ids are also file names.</param>
    /// <returns>Such as <c>old_mill</c>, or <c>old_mill_2</c> when that is taken.</returns>
    public static string FromName(string name, IReadOnlyCollection<string> taken)
    {
        var baseId = SnakeCase(name);
        var takenIds = taken.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var id = baseId;
        for (var suffix = 2; takenIds.Contains(id); suffix++)
        {
            id = $"{baseId}_{suffix}";
        }

        return id;
    }

    // Letters and digits kept in lower case; every run of anything else becomes one underscore between words.
    private static string SnakeCase(string name)
    {
        var id = new StringBuilder();
        foreach (var character in name.Trim())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                id.Append(char.ToLowerInvariant(character));
            }
            else if (id.Length > 0 && id[^1] != '_')
            {
                id.Append('_');
            }
        }

        var snakeCase = id.ToString().TrimEnd('_');
        return snakeCase.Length > 0 ? snakeCase : Fallback;
    }
}
