using Jrpg.Core.Events;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// An openShop step names its shop (ADR 0031); the dialog picks it from the loaded shops and refuses a step without one.
public sealed class EventStepViewModelShopTests
{
    [Fact]
    public void FromStep_OpenShopStep_ShowsItsShop()
    {
        var step = EventStepViewModel.From(new OpenShopStep("general_store"));

        Assert.Equal("Open shop: general_store", step.Label);
        Assert.Equal("general_store", step.ShopId);
        Assert.True(step.IsOpenShop);
        Assert.Null(step.Problem);
    }

    [Fact]
    public void ToStep_OpenShop_ReturnsStepWithTheShop()
    {
        var step = EventStepViewModel.New(EventStepKind.OpenShop);
        step.ShopId = "general_store";

        Assert.Equal(new OpenShopStep("general_store"), step.ToStep());
    }

    [Fact]
    public void Problem_OpenShopWithoutShop_AsksForIt()
    {
        var step = EventStepViewModel.New(EventStepKind.OpenShop);

        Assert.Equal("Choose the shop.", step.Problem);
        Assert.Throws<InvalidOperationException>(step.ToStep);
    }
}
