using Jrpg.Core.Events;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The start map's shopkeeper (decision 353): a line, then the general store.</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class StartMapShopkeeperTests
{
    [Fact]
    public void Interact_FacingShopkeeper_StartsWithTheirLine()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowShopkeeper);

        Assert.True(exploration.TryInteract());

        Assert.Equal("Shopkeeper", Assert.IsType<DialogueStep>(exploration.CurrentEventStep).Speech.Speaker);
    }

    // Confirm on every other update reads the line through, whatever its typing takes; then the store is asked for.
    [Fact]
    public void Update_TalkThroughTheLine_RequestsTheGeneralStore()
    {
        var exploration = StartMapScenarios.StartExploring(StartMapScenarios.BelowShopkeeper);
        var scene = StartMapScenarios.SceneOf(exploration, StartMapScenarios.BelowShopkeeper);

        var requested = false;
        for (var update = 0; update < 300 && !requested; update++)
        {
            requested = scene.Update(StartMapScenarios.OneUpdate, update % 2 == 0 ? Pressing(GameButton.Confirm) : default) is ShopRequested { ShopId: "general_store" };
        }

        Assert.True(requested);
        Assert.IsType<OpenShopStep>(exploration.CurrentEventStep);
    }
}
