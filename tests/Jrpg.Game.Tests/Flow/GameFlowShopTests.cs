using Jrpg.Core.Events;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Trade;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

// The chest opens the general store, then says a line: the Shop opens and closes without the flow's fade, fading itself
// out over the map first (decision 370), and the event goes on.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowShopTests
{
    [Fact]
    public void Update_LeaveTheShop_ContinuesTheEvent()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.BelowShopChest);
        var session = SessionOf(flow);
        flow.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(StartMapScenarios.OneUpdate, default);
        Assert.IsType<OpenShopStep>(session.Exploration.CurrentEventStep);

        flow.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Cancel));
        for (var update = 0; update < ShopSceneTests.ClosingFadeUpdates; update++)
        {
            flow.Update(StartMapScenarios.OneUpdate, default);
        }

        Assert.IsType<DialogueStep>(session.Exploration.CurrentEventStep);
    }

}
