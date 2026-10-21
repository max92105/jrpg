using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Session;
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

namespace Jrpg.Game.Tests.Flow;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowTests : IDisposable
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    // The flow's screen-change fade: 0.5 s out, 0.15 s on black, 0.5 s in.
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan _wholeFade = TimeSpan.FromSeconds(1.15);
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

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

        flow.Update(_wholeFade, default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_QuitConfirmed_ReturnsQuitRequest()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Down));

        var request = flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<QuitRequested>(request);
    }

    [Fact]
    public void Update_NewGameConfirmedFadeNotYetBlack_HasNoSession()
    {
        var flow = FlowWithNewGameConfirmed();

        flow.Update(_fadeOut - TimeSpan.FromSeconds(0.1), default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_NewGameConfirmedFadeReachesBlack_StartsOnTheStartMap()
    {
        var flow = FlowWithNewGameConfirmed();

        flow.Update(_fadeOut, default);

        Assert.Equal("start", flow.Session?.Exploration.Location.MapId);
    }

    [Fact]
    public void Update_NewGameConfirmedThenOneUpdateLongerThanTheFade_StillStartsNewGame()
    {
        var flow = FlowWithNewGameConfirmed();

        flow.Update(_wholeFade + TimeSpan.FromSeconds(1), default);

        Assert.NotNull(flow.Session);
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
        var flow = FlowWithNewGameConfirmed();
        flow.Update(_fadeOut, default);
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
        flow.Update(_oneUpdate, InputFrames.PressingEscape);
        // System is the last tab: Up wraps to it from the first.
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Up));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Left));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));

        flow.Update(_fadeOut, default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_NewGameAfterTitleRequested_StartsFreshSession()
    {
        var flow = StartedFlow();
        var first = SessionOf(flow);
        flow.Update(_oneUpdate, InputFrames.PressingEscape);
        // System is the last tab: Up wraps to it from the first.
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Up));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Left));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_wholeFade, default);

        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_fadeOut, default);

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

        flow.Update(_oneUpdate, InputFrames.PressingEscape);
        // System is the last tab: Up wraps to it from the first.
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Up));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_fadeOut, default);

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
            flow.Update(_oneUpdate, StartMapScenarios.HoldingLeft);
        }

        Assert.NotNull(session.ActiveBattle);
        var battlePosition = session.Exploration.PlayerPosition;
        flow.Update(TimeSpan.FromSeconds(10), default);
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Up));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(TimeSpan.FromSeconds(0.5), default);
        Assert.Null(session.ActiveBattle);

        // "Escaped" shows for 1.5 s before the fade back to the map.
        flow.Update(TimeSpan.FromSeconds(1.5), default);
        flow.Update(_wholeFade, default);
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
            flow.Update(_oneUpdate, StartMapScenarios.HoldingLeft);
        }

        // The battle starts at the fade's black and, lost already, ends on the screen's first update.
        Assert.NotNull(session.Exploration.PendingEncounter);
        flow.Update(TimeSpan.FromSeconds(10), default);
        Assert.Null(session.Exploration.PendingEncounter);
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));

        flow.Update(_wholeFade, default);

        Assert.Null(flow.Session);
    }

    // New game is the title's first entry, so Confirm alone picks it; the fade starts on that update.
    private static GameFlow FlowWithNewGameConfirmed(SaveSlots? slots = null, ContentCatalog? catalog = null)
    {
        var flow = new GameFlow(catalog ?? _catalog, RepositoryScreenText.Text, slots ?? UnwrittenSaveSlots.Create(), new PlayerSettings());
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        return flow;
    }

    // A new game whose fade is over, so the map gets input.
    private static GameFlow StartedFlow(SaveSlots? slots = null, ContentCatalog? catalog = null)
    {
        var flow = FlowWithNewGameConfirmed(slots, catalog);
        flow.Update(_fadeOut, default);
        flow.Update(_wholeFade - _fadeOut, default);
        return flow;
    }

    private static GameSession SessionOf(GameFlow flow) =>
        flow.Session ?? throw new InvalidOperationException("The flow has no session yet.");
}
