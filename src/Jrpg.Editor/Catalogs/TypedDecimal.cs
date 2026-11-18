using System.Globalization;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// A decimal number as a form keeps it, as <see cref="TypedNumber"/> keeps a whole one: the text as typed, a dot for
/// decimals and the sign allowed, since Data says which positions may be negative, and an exponent, since
/// <see cref="Text"/> writes a tiny or huge number as 1E-05. "NaN" and "Infinity", which
/// <see cref="double.TryParse(string, NumberStyles, IFormatProvider, out double)"/> reads whatever the styles, are
/// refused: the content can't hold them and the save would throw.
/// </summary>
internal static class TypedDecimal
{
    private const NumberStyles Style =
        NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowExponent;

    /// <returns>The number, or null when the text isn't a finite decimal number.</returns>
    public static double? Parse(string text) =>
        double.TryParse(text.Trim(), Style, CultureInfo.InvariantCulture, out var number) && double.IsFinite(number)
            ? number
            : null;

    public static string Text(double number) => number.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Why a field can't be read: "X must be a number, with a dot for decimals (was 'NaN')."; null when it can.
    /// </summary>
    public static string? ProblemOf(string label, string text) =>
        Parse(text) is null ? $"{label} must be a number, with a dot for decimals (was '{text.Trim()}')." : null;
}
