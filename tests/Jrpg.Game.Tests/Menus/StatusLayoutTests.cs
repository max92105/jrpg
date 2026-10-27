using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;

namespace Jrpg.Game.Tests.Menus;

public sealed class StatusLayoutTests
{
    [Fact]
    public void Resources_EveryClassInContent_FitTheBand()
    {
        var mostResources = StartMapScenarios.BelowClosedDoor.Classes.Values.Max(characterClass => characterClass.Resources.Count);

        Assert.True(
            mostResources <= StatusParametersView.MostResources,
            $"A class has {mostResources} resources; the Status page's band has room for {StatusParametersView.MostResources}.");
    }
}
