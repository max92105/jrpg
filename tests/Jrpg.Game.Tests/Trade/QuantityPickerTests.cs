using Jrpg.Core.Trade;
using Jrpg.Game.Trade;

namespace Jrpg.Game.Tests.Trade;

// Up and Down wrap between 1 and the most; the steps by ten stop at the ends (decision 377).
public sealed class QuantityPickerTests
{
    [Fact]
    public void Up_AtTheMost_WrapsToOne()
    {
        var picker = new QuantityPicker(3, FlatQuote);
        picker.Up();
        picker.Up();

        picker.Up();

        Assert.Equal(1, picker.Count);
    }

    [Fact]
    public void Down_AtOne_WrapsToTheMost()
    {
        var picker = new QuantityPicker(5, FlatQuote);

        picker.Down();

        Assert.Equal(5, picker.Count);
    }

    [Fact]
    public void StepUp_NearTheMost_StopsAtTheMost()
    {
        var picker = new QuantityPicker(15, FlatQuote);
        picker.StepUp();
        Assert.Equal(11, picker.Count);

        picker.StepUp();

        Assert.Equal(15, picker.Count);
    }

    [Fact]
    public void StepDown_NearOne_StopsAtOne()
    {
        var picker = new QuantityPicker(15, FlatQuote);
        picker.StepUp();
        picker.Down();

        picker.StepDown();

        Assert.Equal(1, picker.Count);
    }

    [Fact]
    public void Up_TotalFollowsTheQuote()
    {
        var picker = new QuantityPicker(9, FlatQuote);
        Assert.Equal(7, picker.Quote.Gold);

        picker.Up();

        Assert.Equal(14, picker.Quote.Gold);
    }

    [Fact]
    public void Constructor_NoMost_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new QuantityPicker(0, FlatQuote));

    private static TradeQuote FlatQuote(int count) => new(count * 7, null);
}
