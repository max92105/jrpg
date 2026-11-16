using System.Globalization;
using System.Windows.Data;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Statuses;

namespace Jrpg.Editor.Tests.Statuses;

// The Acts at radio buttons: each checked for its own timing, only a check written back.
public sealed class TimingButtonConverterTests
{
    private readonly TimingButtonConverter _converter = new();

    [Fact]
    public void Convert_TheButtonsTiming_IsChecked()
    {
        var isChecked = CheckOf(StatusTiming.StartOfTurn, button: StatusTiming.StartOfTurn);

        Assert.Equal(true, isChecked);
    }

    [Fact]
    public void Convert_TheOtherTiming_IsUnchecked()
    {
        var isChecked = CheckOf(StatusTiming.EndOfTurn, button: StatusTiming.StartOfTurn);

        Assert.Equal(false, isChecked);
    }

    [Fact]
    public void ConvertBack_Checked_IsTheButtonsTiming()
    {
        var timing = TimingOf(true, button: StatusTiming.EndOfTurn);

        Assert.Equal(StatusTiming.EndOfTurn, timing);
    }

    [Fact]
    public void ConvertBack_Unchecked_WritesNothing()
    {
        var timing = TimingOf(false, button: StatusTiming.EndOfTurn);

        Assert.Same(Binding.DoNothing, timing);
    }

    private object CheckOf(StatusTiming timing, StatusTiming button) =>
        _converter.Convert(timing, typeof(bool?), button, CultureInfo.InvariantCulture);

    private object? TimingOf(bool isChecked, StatusTiming button) =>
        _converter.ConvertBack(isChecked, typeof(StatusTiming), button, CultureInfo.InvariantCulture);
}
