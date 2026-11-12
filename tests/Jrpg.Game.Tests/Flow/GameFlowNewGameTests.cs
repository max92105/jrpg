using Jrpg.Game.Flow;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

/// <summary>From the title's New game through the leader choice and the intro to the start map.</summary>
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowNewGameTests
{
    [Fact]
    public void Update_NewGameConfirmedWholeFade_HasNoSession()
    {
        var flow = new GameFlow(StartMapScenarios.TestWorld, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        flow.Update(WholeFade, default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_CancelOnChoice_ReturnsToTitleWithoutSession()
    {
        var flow = new GameFlow(StartMapScenarios.TestWorld, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(WholeFade, default);
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Cancel));
        flow.Update(WholeFade, default);

        // With no save the title lists New Game and Quit: Down then Confirm quits only if the title is back.
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));
        var request = flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<QuitRequested>(request);
        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_LeaderConfirmedFadeReachesBlack_StartsLedByThem()
    {
        var catalog = StartMapScenarios.TestWorld with { NewGame = StartMapScenarios.TestWorld.NewGame with { LeaderChoices = [Choice("sarxian"), Choice("zanyr")] } };
        var flow = new GameFlow(catalog, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(WholeFade, default);
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        AnswerYes(flow);

        flow.Update(FadeOut, default);

        Assert.Equal("zanyr", SessionOf(flow).Party.Leader.Id);
    }

    [Fact]
    public void Update_LeaderConfirmedFadeNotYetBlack_HasNoSession()
    {
        var flow = FlowWithLeaderConfirmed();

        flow.Update(FadeOut - TimeSpan.FromSeconds(0.1), default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_LeaderConfirmedFadeReachesBlack_StartsOnTheStartMap()
    {
        var flow = FlowWithLeaderConfirmed();

        flow.Update(FadeOut, default);

        Assert.Equal("start", flow.Session?.Exploration.Location.MapId);
    }

    [Fact]
    public void Update_TestWorldIntroSkipped_LandsOnTheStartMap()
    {
        var flow = FlowWithLeaderConfirmed(catalog: StartMapScenarios.TestWorld);
        flow.Update(WholeFade, default);
        flow.Update(OneUpdate, default);
        Assert.Equal("intro", SessionOf(flow).Exploration.CurrentMap.Id);

        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        for (var update = 0; update < 60; update++)
        {
            flow.Update(OneUpdate, default);
        }

        Assert.Equal("start", SessionOf(flow).Exploration.CurrentMap.Id);
    }

    [Fact]
    public void Update_LeaderConfirmedThenOneUpdateLongerThanTheFade_StillStartsNewGame()
    {
        var flow = FlowWithLeaderConfirmed();

        flow.Update(WholeFade + TimeSpan.FromSeconds(1), default);

        Assert.NotNull(flow.Session);
    }

    [Fact]
    public void Update_IntroSkipped_VoiceStops()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.TestWorld);
        flow.Update(OneUpdate, default);
        Assert.Equal("Placeholder.wav", flow.Audio.Voice.VoiceFile);

        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.Null(flow.Audio.Voice.VoiceFile);
    }
}
