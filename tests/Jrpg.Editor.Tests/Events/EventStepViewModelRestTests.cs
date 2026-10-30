using Jrpg.Core.Events;
using Jrpg.Core.Party;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// A rest step's price (decision 382): empty for a free rest, else a whole number the purse can hold.
public sealed class EventStepViewModelRestTests
{
    [Fact]
    public void FromStep_PricedRest_ShowsItsPrice()
    {
        var step = EventStepViewModel.From(new RestStep { Price = 20 });

        Assert.Equal("20", step.PriceText);
        Assert.Equal("Rest for 20 gold", step.Label);
        Assert.Null(step.Problem);
    }

    [Fact]
    public void ToStep_EmptyPrice_ReturnsFreeRest()
    {
        var step = EventStepViewModel.From(new RestStep { Price = 20 });
        step.PriceText = " ";

        Assert.Equal("Rest", step.Label);
        Assert.Equal(new RestStep(), step.ToStep());
    }

    [Fact]
    public void ToStep_Price_ReturnsPricedRest()
    {
        var step = EventStepViewModel.New(EventStepKind.Rest);
        step.PriceText = " 35 ";

        Assert.Null(step.Problem);
        Assert.Equal(new RestStep { Price = 35 }, step.ToStep());
    }

    [Fact]
    public void Problem_ZeroPrice_Explains()
    {
        var step = EventStepViewModel.New(EventStepKind.Rest);
        step.PriceText = "0";

        Assert.Equal("The price must be empty or a whole number from 1 to 9999999.", step.Problem);
        Assert.Throws<InvalidOperationException>(step.ToStep);
    }

    [Fact]
    public void Problem_PriceAboveMaxGold_Explains()
    {
        var step = EventStepViewModel.New(EventStepKind.Rest);
        step.PriceText = (Purse.MaxGold + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

        Assert.NotNull(step.Problem);
    }

    [Fact]
    public void Problem_PriceAtMaxGold_IsNull()
    {
        var step = EventStepViewModel.New(EventStepKind.Rest);
        step.PriceText = Purse.MaxGold.ToString(System.Globalization.CultureInfo.InvariantCulture);

        Assert.Null(step.Problem);
    }
}
