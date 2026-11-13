using System.Globalization;
using System.Windows.Data;

namespace Jrpg.Editor.Sprites;

/// <summary>
/// A walk row box's index to and from its row: the -1 a combo box writes back while its list is rebuilt (the rows
/// retyped, another image chosen) is dropped, so a row is never lost to the view's own churn.
/// </summary>
internal sealed class RowIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int index && index >= 0 ? index : Binding.DoNothing;
}
