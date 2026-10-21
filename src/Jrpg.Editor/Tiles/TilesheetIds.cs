using System.Globalization;
using System.IO;
using System.Text;

namespace Jrpg.Editor.Tiles;

/// <summary>A new sheet's id, from its image's file name (decision 118).</summary>
internal static class TilesheetIds
{
    /// <summary>
    /// The file name without its extension in snake case, digits split from letters (<c>Grass3.png</c> -> <c>grass_3</c>,
    /// <c>RA_Jungle.png</c> -> <c>ra_jungle</c>); <c>_2</c>, <c>_3</c>... appended while the id is taken.
    /// </summary>
    /// <param name="file">The image's file name in the tilesheet folder.</param>
    /// <param name="takenIds">The sheet ids already in the catalog.</param>
    public static string FromFile(string file, IReadOnlyCollection<string> takenIds)
    {
        var id = SnakeCaseOf(Path.GetFileNameWithoutExtension(file));
        var unique = id;
        for (var number = 2; takenIds.Contains(unique); number++)
        {
            unique = string.Create(CultureInfo.InvariantCulture, $"{id}_{number}");
        }

        return unique;
    }

    // Anything but a letter or digit becomes an underscore, as ids hold only those.
    private static string SnakeCaseOf(string name)
    {
        var id = new StringBuilder();
        for (var index = 0; index < name.Length; index++)
        {
            if (index > 0 && StartsWord(name, index) && id[^1] != '_')
            {
                id.Append('_');
            }

            id.Append(char.IsAsciiLetterOrDigit(name[index]) ? char.ToLowerInvariant(name[index]) : '_');
        }

        return id.ToString();
    }

    // "Grass3" splits before the digit, "tree02s" before and after it, "DarkCave" before "C", "RAJungle" before "J".
    private static bool StartsWord(string name, int index)
    {
        var previous = name[index - 1];
        var current = name[index];
        var isNextLower = index + 1 < name.Length && char.IsLower(name[index + 1]);
        return (char.IsLetter(previous) && char.IsDigit(current))
            || (char.IsDigit(previous) && char.IsLetter(current))
            || (char.IsLower(previous) && char.IsUpper(current))
            || (char.IsUpper(previous) && char.IsUpper(current) && isNextLower);
    }
}
