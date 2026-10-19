using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Session;
using Jrpg.Data.Content;
using Jrpg.Game.Flow;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Flow;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    // The flow's screen-change fade: 0.5 s out, 0.15 s on black, 0.5 s in.
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.5);
    private static readonly TimeSpan _wholeFade = TimeSpan.FromSeconds(1.15);
    private static readonly TimeSpan _oneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    private static readonly InputFrame _holdingRight =
        new(MoveIntent.FromAxes(1, 0), ButtonSet.Empty.With(GameButton.Right), ButtonSet.Empty);

    [Fact]
    public void Constructor_WaitingOnTitle_HasNoSession()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text);

        flow.Update(_wholeFade, default);

        Assert.Null(flow.Session);
    }

    [Fact]
    public void Update_QuitConfirmed_ReturnsQuitRequest()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text);
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
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
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
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Down));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Left));
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_wholeFade, default);

        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(_fadeOut, default);

        Assert.NotSame(first, SessionOf(flow));
    }

    // New game is the title's first entry, so Confirm alone picks it; the fade starts on that update.
    private static GameFlow FlowWithNewGameConfirmed()
    {
        var flow = new GameFlow(_catalog, RepositoryScreenText.Text);
        flow.Update(_oneUpdate, InputFrames.Pressing(GameButton.Confirm));
        return flow;
    }

    // A new game whose fade is over, so the map gets input.
    private static GameFlow StartedFlow()
    {
        var flow = FlowWithNewGameConfirmed();
        flow.Update(_fadeOut, default);
        flow.Update(_wholeFade - _fadeOut, default);
        return flow;
    }

    private static GameSession SessionOf(GameFlow flow) =>
        flow.Session ?? throw new InvalidOperationException("The flow has no session yet.");
}
