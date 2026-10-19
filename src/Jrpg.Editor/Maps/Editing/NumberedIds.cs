using System.Globalization;

namespace Jrpg.Editor.Maps.Editing;

/// <summary>
/// The numbering object uids and event ids share: a prefix, then a number. Both schemes count from the highest number
/// in use, so they read it the same way.
/// </summary>
internal static class NumberedIds
{
    /// <summary>
    /// The highest number among the ids that are <paramref name="prefix"/> followed by digits only; null when none are.
    /// </summary>
    /// <param name="prefix">Such as <c>chest_</c>.</param>
    /// <param name="ids">The ids in use.</param>
    public static int? HighestNumber(string prefix, IEnumerable<string> ids)
    {
        int? highest = null;
        foreach (var id in ids)
        {
            if (id.StartsWith(prefix, StringComparison.Ordinal)
                && int.TryParse(id.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                && number > (highest ?? -1))
            {
                highest = number;
            }
        }

        return highest;
    }
}
