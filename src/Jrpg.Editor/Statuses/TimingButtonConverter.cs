using System.Globalization;
using System.Windows.Data;
using Jrpg.Core.Statuses;

namespace Jrpg.Editor.Statuses;

/// <summary>
/// An Acts at radio button's check from the status's timing, its parameter the button's timing (decision 695). Only a
/// check is written back: the unchecking WPF does to the other button of the group, which may still be bound to the
/// status chosen before, is dropped, so choosing a status never changes another's timing.
/// </summary>
internal sealed class TimingButtonConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is StatusTiming timing && parameter is StatusTiming button && timing == button;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true && parameter is StatusTiming button ? button : Binding.DoNothing;
}
