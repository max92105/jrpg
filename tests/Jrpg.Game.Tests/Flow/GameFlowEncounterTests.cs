using Jrpg.Game.Flow;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Flow;

/// <summary>An encounter on the field (decision 258): the flash over the frozen frame comes before the fade.</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowEncounterTests
{
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan _flash = TimeSpan.FromSeconds(0.12);

    [Fact]
    public void Update_BattleRequested_FlashesBeforeTheFade()
    {
        var flow = FlowAtAnEncounter();
        var session = GameFlowTests.SessionOf(flow);

        RunFor(flow, _fadeOut);
        var startedWithoutTheFlash = session.ActiveBattle is not null;
        RunFor(flow, _flash + _oneUpdate);

        Assert.False(startedWithoutTheFlash);
        Assert.NotNull(session.ActiveBattle);
    }

    [Fact]
    public void Update_BattleRequested_WantsAFrozenFrame()
    {
        var flow = GameFlowTests.StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        var wantedBefore = flow.WantsFrozenFrame;

        WalkToAnEncounter(flow);

        Assert.False(wantedBefore);
        Assert.True(flow.WantsFrozenFrame);
    }

    [Fact]
    public void Update_FlashOver_NoLongerWantsAFrozenFrame()
    {
        var flow = FlowAtAnEncounter();

        RunFor(flow, _flash + _oneUpdate);

        Assert.False(flow.WantsFrozenFrame);
    }

    private static GameFlow FlowAtAnEncounter()
    {
        var flow = GameFlowTests.StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        WalkToAnEncounter(flow);
        return flow;
    }

    // The forest always rolls an encounter, so walking left reaches one; the update that asks for the battle starts the
    // flash, which wants the frozen frame from then on.
    private static void WalkToAnEncounter(GameFlow flow)
    {
        for (var update = 0; update < 600 && !flow.WantsFrozenFrame; update++)
        {
            flow.Update(_oneUpdate, StartMapScenarios.HoldingLeft);
        }
    }

    private static void RunFor(GameFlow flow, TimeSpan length)
    {
        for (var elapsed = TimeSpan.Zero; elapsed < length; elapsed += _oneUpdate)
        {
            flow.Update(_oneUpdate, default);
        }
    }
}
