using Jrpg.Core.Events;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

// The chest opens the Forge, then says a line: the Forge opens and closes without the flow's fade (decision 370).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowForgeTests
{
    [Fact]
    public void Update_CancelInTheForge_ContinuesTheEvent()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.BelowForgeChest);
        var session = SessionOf(flow);
        flow.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(StartMapScenarios.OneUpdate, default);
        Assert.IsType<OpenForgeStep>(session.Exploration.CurrentEventStep);

        flow.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Cancel));

        Assert.IsType<DialogueStep>(session.Exploration.CurrentEventStep);
    }

    // The Forge is on top: the Confirm that would read on at the map goes to the Forge, so the event stays on its step.
    [Fact]
    public void Update_ConfirmInTheForge_EventStaysOnTheForgeStep()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.BelowForgeChest);
        var session = SessionOf(flow);
        flow.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(StartMapScenarios.OneUpdate, default);

        flow.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<OpenForgeStep>(session.Exploration.CurrentEventStep);
    }
}
