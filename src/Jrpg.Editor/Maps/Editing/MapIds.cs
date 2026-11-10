using System.Text;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// Ids for maps, made from the name typed for them: snake_case like the content's other ids, and unique. A map keeps
/// its id until Map > Rename map changes it with every reference (decision 530).
/// </summary>
internal static class MapIds
{
    // When the name has no letters or digits at all.
    private const string Fallback = "map";

    /// <summary>
    /// Whether a typed id is one this rule makes: lower-case letters, digits and underscores, not empty.
    /// </summary>
    public static bool IsWellFormed(string id) =>
        id.Length > 0 && id.All(character => char.IsAsciiLetterLower(character) || char.IsAsciiDigit(character) || character == '_');

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
