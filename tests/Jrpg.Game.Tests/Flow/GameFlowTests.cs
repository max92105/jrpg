using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Data.Saving;
using Jrpg.Game.Flow;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Saving;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowTests : IDisposable
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    private static readonly InputFrame _holdingRight =
        new(MoveIntent.FromAxes(1, 0), ButtonSet.Empty.With(GameButton.Right), ButtonSet.Empty);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-flow-saves-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void Constructor_WaitingOnTitle_HasNoSession()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());

        flow.Update(WholeFade, default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_QuitConfirmed_ReturnsQuitRequest()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));

        var request = flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<QuitRequested>(request);
    }

    [Fact]
    public void Update_NewGameConfirmedWholeFade_HasNoSession()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        flow.Update(WholeFade, default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_CancelOnChoice_ReturnsToTitleWithoutSession()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());
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
        var catalog = _catalog with { NewGame = _catalog.NewGame with { LeaderChoices = [Choice("sarxian"), Choice("zanyr")] } };
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
    public void Update_RepositoryIntroSkipped_LandsOnTheStartMap()
    {
        var flow = FlowWithLeaderConfirmed(catalog: _catalog);
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
        var flow = StartedFlow(catalog: _catalog);
        flow.Update(OneUpdate, default);
        Assert.Equal("Placeholder.wav", flow.Voice.VoiceFile);

        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.Null(flow.Voice.VoiceFile);
    }

    [Fact]
    public void Update_TwoDeltasAfterStart_AddsBothToPlayTime()
    {
        var flow = StartedFlow();
        var before = SessionOf(flow).PlayTime;

        flow.Update(TimeSpan.FromSeconds(0.5), default);
        flow.Update(TimeSpan.FromSeconds(0.25), default);

        Assert.Equal(TimeSpan.FromSeconds(0.75), SessionOf(flow).PlayTime - before);
    }

    [Fact]
    public void Update_DuringFade_IgnoresInput()
    {
        var flow = FlowWithLeaderConfirmed();
        flow.Update(FadeOut, default);
        var start = SessionOf(flow).Exploration.PlayerPosition;

        flow.Update(TimeSpan.FromSeconds(0.1), _holdingRight);

        Assert.Equal(start, SessionOf(flow).Exploration.PlayerPosition);
    }

    [Fact]
    public void Update_HoldingRightAfterFade_MovesPlayerRight()
    {
        var flow = StartedFlow();
        var start = SessionOf(flow).Exploration.PlayerPosition;

        flow.Update(TimeSpan.FromSeconds(0.1), _holdingRight);

        var exploration = SessionOf(flow).Exploration;
        Assert.True(exploration.PlayerPosition.X > start.X, $"Expected the player right of {start.X}, at {exploration.PlayerPosition.X}.");
        Assert.Equal(start.Y, exploration.PlayerPosition.Y);
        Assert.Equal(Facing.Right, exploration.PlayerFacing);
    }

    [Fact]
    public void Update_TitleRequested_EndsSession()
    {
        var flow = StartedFlow();
        flow.Update(OneUpdate, InputFrames.PressingEscape);
        // System is the last tab: Up wraps to it from the first.
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Up));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Left));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        flow.Update(FadeOut, default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_NewGameAfterTitleRequested_StartsFreshSession()
    {
        var flow = StartedFlow();
        var first = SessionOf(flow);
        flow.Update(OneUpdate, InputFrames.PressingEscape);
        // System is the last tab: Up wraps to it from the first.
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Up));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Left));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(WholeFade, default);

        ConfirmNewGameAndLeader(flow);
        flow.Update(FadeOut, default);

        Assert.NotSame(first, SessionOf(flow));
    }

    [Fact]
    public void Update_LoadRequested_RestoresSavedLocationAndPlayTime()
    {
        var slots = new SaveSlots(new FileSaveStore(_directory, _catalog), _catalog, () => DateTimeOffset.UnixEpoch);
        var flow = StartedFlow(slots);
        flow.Update(TimeSpan.FromSeconds(0.3), _holdingRight);
        var saved = SessionOf(flow);
        slots.Write(1, saved);
        var (savedPosition, savedPlayTime) = (saved.Exploration.PlayerPosition, saved.PlayTime);
        flow.Update(TimeSpan.FromSeconds(0.3), _holdingRight);
        Assert.NotEqual(savedPosition, SessionOf(flow).Exploration.PlayerPosition);

        flow.Update(OneUpdate, InputFrames.PressingEscape);
        // System is the last tab: Up wraps to it from the first.
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Up));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(FadeOut, default);

        var loaded = SessionOf(flow);
        Assert.NotSame(saved, loaded);
        Assert.Equal("start", loaded.Exploration.Location.MapId);
        Assert.Equal(savedPosition, loaded.Exploration.PlayerPosition);
        Assert.Equal(savedPlayTime, loaded.PlayTime);
    }

    // ADR 0008's headless test. Seed-independent: Run always works, and the goblins' first turns can't fell Sarxian.
    [Fact]
    public void Update_WalkIntoForestThatAlwaysRolls_FightsAndReturnsToMap()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        var session = SessionOf(flow);
        for (var update = 0; update < 600 && session.ActiveBattle is null; update++)
        {
            flow.Update(OneUpdate, StartMapScenarios.HoldingLeft);
        }

        Assert.NotNull(session.ActiveBattle);
        var battlePosition = session.Exploration.PlayerPosition;
        flow.Update(TimeSpan.FromSeconds(10), default);
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Up));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(TimeSpan.FromSeconds(0.5), default);
        Assert.Null(session.ActiveBattle);

        // "Escaped" shows for 1.5 s before the fade back to the map.
        flow.Update(TimeSpan.FromSeconds(1.5), default);
        flow.Update(WholeFade, default);
        Assert.Equal(battlePosition, session.Exploration.PlayerPosition);

        flow.Update(TimeSpan.FromSeconds(0.1), StartMapScenarios.HoldingLeft);

        Assert.NotEqual(battlePosition, session.Exploration.PlayerPosition);
    }

    // The whole party is down before the forest, so the battle is lost at its start (decision 97), and with no save to
    // load the defeat screen's cursor starts on Main menu.
    [Fact]
    public void Update_DefeatThenMainMenu_EndsSession()
    {
        var flow = StartedFlow(catalog: StartMapScenarios.NextToForestWarpAlwaysRolling);
        var session = SessionOf(flow);
        session.Party.Leader.Vitals.TakeDamage(9999);
        for (var update = 0; update < 600 && session.Exploration.PendingEncounter is null; update++)
        {
            flow.Update(OneUpdate, StartMapScenarios.HoldingLeft);
        }

        // The battle starts at the fade's black and, lost already, ends on the screen's first update.
        Assert.NotNull(session.Exploration.PendingEncounter);
        flow.Update(TimeSpan.FromSeconds(10), default);
        Assert.Null(session.Exploration.PendingEncounter);
        flow.Update(TimeSpan.FromSeconds(1), default); // The defeat screen's choices come in before they take input.
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        flow.Update(WholeFade, default);

        Assert.Null(flow.Session);
    }
}
