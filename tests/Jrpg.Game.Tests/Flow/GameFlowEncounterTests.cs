using Jrpg.Game.Flow;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

// GameFlowTests is over 300 lines, so the encounter's flash is tested here. Seed-independent: the forest always rolls.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowEncounterTests
{
    // The flash takes 0.12 s, then the fade out; a fade started with the flash would have reached black by then.
    private static readonly TimeSpan _flashThenMostOfTheFadeOut = TimeSpan.FromSeconds(0.55);

    [Fact]
    public void Update_BattleRequested_WantsAFrozenFrame()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);

        WalkUntilAnEncounter(flow);

        Assert.True(flow.WantsFrozenFrame);
        Assert.Null(SessionOf(flow).ActiveBattle);
    }

    // Stepped at the host's rate, so the fade only gets the time left after the flash.
    [Fact]
    public void Update_BattleRequested_FlashesBeforeTheFade()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        WalkUntilAnEncounter(flow);

        StepFor(flow, _flashThenMostOfTheFadeOut);
        var hasBattleBeforeTheFadeEnds = SessionOf(flow).ActiveBattle is not null;
        StepFor(flow, FadeOut);

        Assert.False(hasBattleBeforeTheFadeEnds);
        Assert.NotNull(SessionOf(flow).ActiveBattle);
    }

    private static void StepFor(GameFlow flow, TimeSpan length)
    {
        for (var elapsed = TimeSpan.Zero; elapsed < length; elapsed += OneUpdate)
        {
            flow.Update(OneUpdate, default);
        }
    }
}
