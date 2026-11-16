using System.Globalization;
using System.Text.RegularExpressions;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// The id of a record's next tier (decision 985), for the Effects page's Duplicate and the Statuses page's New tier:
/// the tier is the id's trailing <c>_&lt;number&gt;</c>, as the content names them (<c>fire_1</c>, <c>poison_2</c>).
/// </summary>
internal static partial class TierIds
{
    /// <returns>
    /// The id without its trailing <c>_&lt;digits&gt;</c>: <c>fire</c> from <c>fire_1</c>, <c>stun</c> from
    /// <c>stun</c>.
    /// </returns>
    public static string Stem(string id) => TierNumber().Match(id) is { Success: true } tier ? id[..tier.Index] : id;

    /// <summary>
    /// The stem, an underscore and one more than the highest number of that stem among the taken ids and the chosen
    /// one, a bare stem counting as 0: <c>fire_2</c> from <c>fire_1</c>, <c>stun_1</c> from <c>stun</c>, <c>fire_3</c>
    /// when <c>fire_2</c> is taken.
    /// </summary>
    /// <param name="chosenId">The record the next tier follows.</param>
    /// <param name="takenIds">Every id already typed, which the next tier must not take.</param>
    public static string Next(string chosenId, IEnumerable<string> takenIds)
    {
        var stem = Stem(chosenId);
        var highest = takenIds.Append(chosenId).Select(id => TierOf(id, stem)).Max();

        // Counted past int's range, so an id typed at int.MaxValue gives a longer one rather than a negative number.
        return $"{stem}_{((long)highest + 1).ToString(CultureInfo.InvariantCulture)}";
    }

    // 0 for the bare stem and for any id of another stem, or whose number doesn't fit an int: neither raises the next
    // tier past 1.
    private static int TierOf(string id, string stem)
    {
        var tier = TierNumber().Match(id);
        if (!tier.Success || id[..tier.Index] != stem)
        {
            return 0;
        }

        return int.TryParse(tier.Groups["number"].ValueSpan, CultureInfo.InvariantCulture, out var number) ? number : 0;
    }

    [GeneratedRegex("_(?<number>[0-9]+)$")]
    private static partial Regex TierNumber();
}
