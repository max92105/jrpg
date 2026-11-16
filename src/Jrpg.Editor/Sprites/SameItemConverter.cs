using System.Globalization;
using System.Windows.Data;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// True when its two values are the same object: a facing line's Pick toggle is checked while its facing is the picking
/// one (decision 772).
/// </summary>
internal sealed class SameItemConverter : IMultiValueConverter
{
    public object Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) =>
        values.Length == 2 && values[0] is not null && ReferenceEquals(values[0], values[1]);

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("The toggle runs its command; the binding is one-way.");
}
