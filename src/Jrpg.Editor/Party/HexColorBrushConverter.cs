using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Jrpg.Editor.Party;

/// <summary>
/// A resource's color as typed to its swatch's brush: <c>#RRGGBB</c>, trimmed as the resource writes it, gives that
/// color; anything else, a half-typed color included, gives transparent, so typing never raises a binding error and the
/// problem line names Data's rule. One-way.
/// </summary>
internal sealed class HexColorBrushConverter : IValueConverter
{
    private const int HexColorLength = 7;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && RgbOf(text.Trim()) is { } rgb ? BrushOf(rgb) : Brushes.Transparent;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("The swatch only shows the typed color.");

    // AllowHexSpecifier takes hex digits only, no sign or space, as Data's color rule.
    private static uint? RgbOf(string text) =>
        text.Length == HexColorLength
        && text[0] == '#'
        && uint.TryParse(text.AsSpan(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var rgb)
            ? rgb
            : null;

    private static SolidColorBrush BrushOf(uint rgb)
    {
        var brush = new SolidColorBrush(Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb));
        brush.Freeze();
        return brush;
    }
}
