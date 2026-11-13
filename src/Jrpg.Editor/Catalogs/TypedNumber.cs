using System.Globalization;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// A whole number as a form keeps it: the text as typed, so an emptied or half-typed box is reported instead of keeping
/// a stray value; the sign is allowed, since Data says which numbers may be negative.
/// </summary>
internal static class TypedNumber
{
    /// <returns>The number, or null when the text isn't a whole number.</returns>
    public static int? Parse(string text) =>
        int.TryParse(text.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var number) ? number : null;

    public static string Text(int number) => number.ToString(CultureInfo.InvariantCulture);

    /// <summary>Why a field can't be read: "Max HP must be a whole number (was 'ten')."; null when it can.</summary>
    public static string? ProblemOf(string label, string text) =>
        Parse(text) is null ? $"{label} must be a whole number (was '{text.Trim()}')." : null;
}
