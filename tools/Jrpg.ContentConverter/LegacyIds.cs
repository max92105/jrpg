using System.Text;

namespace Jrpg.ContentConverter;

/// <summary>
/// Turns a legacy display name into a snake_case content id, by one rule for objects and maps alike:
/// spaces, underscores and hyphens separate words, and so does a switch between letters and digits or a
/// lowercase letter followed by an uppercase one. "Chest Opened" -> chest_opened, "House1" -> house_1.
/// </summary>
internal static class LegacyIds
{
    private const char Separator = '_';

    /// <param name="sourceFile">The legacy file the name comes from, for error messages.</param>
    /// <param name="name">The legacy display name or file name without its extension.</param>
    public static string FromName(string sourceFile, string name)
    {
        var id = new StringBuilder();
        var previous = Separator;
        foreach (var character in name)
        {
            if (character is ' ' or '_' or '-')
            {
                AppendSeparator(id);
            }
            else if (char.IsAsciiLetterOrDigit(character))
            {
                if (StartsNewWord(previous, character))
                {
                    AppendSeparator(id);
                }

                id.Append(char.ToLowerInvariant(character));
            }
            else
            {
                throw new ConversionException($"{sourceFile}: name '{name}' has '{character}', which an id can't hold.");
            }

            previous = character;
        }

        var result = id.ToString().TrimEnd(Separator);
        return result.Length > 0 ? result : throw new ConversionException($"{sourceFile}: name '{name}' gives an empty id.");
    }

    private static bool StartsNewWord(char previous, char current) =>
        (char.IsAsciiLetter(previous) && char.IsAsciiDigit(current))
        || (char.IsAsciiDigit(previous) && char.IsAsciiLetter(current))
        || (char.IsAsciiLetterLower(previous) && char.IsAsciiLetterUpper(current));

    private static void AppendSeparator(StringBuilder id)
    {
        if (id.Length > 0 && id[^1] != Separator)
        {
            id.Append(Separator);
        }
    }
}
