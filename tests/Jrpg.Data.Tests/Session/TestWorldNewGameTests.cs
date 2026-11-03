using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Session;

/// <summary>The new game's map mechanisms on the frozen test world (ADR 0037), where a cell can be pinned.</summary>
public sealed class TestWorldNewGameTests
{
    private static readonly string _testWorldRoot = Path.Combine(AppContext.BaseDirectory, "test-world");

    [Fact]
    public void StartNew_TestWorld_EveryLeaderHasOneIntroEndingOnTheStartMap()
    {
        var catalog = ContentCatalogLoader.Load(_testWorldRoot);
        var arrivals = catalog.Maps["intro"].Layout.CellEvents.Select(cellEvent => cellEvent.Event).Where(mapEvent => mapEvent.Activation == EventActivation.Arrive);

        var introsPerLeader = catalog.NewGame.LeaderChoices.Select(choice => arrivals.Where(mapEvent => mapEvent.OnlyWhenLeaderId == choice.MemberId).ToList());

        Assert.All(introsPerLeader, intros => Assert.Equal(new WarpStep("start", new GridCell(23, 20)), Assert.Single(intros).Steps[^1]));
    }

    [Fact]
    public void StartNew_TestWorld_SkippedIntroWarpsToTheStartMap()
    {
        var catalog = ContentCatalogLoader.Load(_testWorldRoot);
        var exploration = GameSession.StartNew(catalog, "thaali", new SeededRandom(0)).Exploration;

        exploration.SkipEventPresentation();
        exploration.ContinueEvent();

        Assert.False(exploration.IsEventRunning);
        Assert.Equal(new MapLocation("start", new Point2D(23.5, 20.5), Facing.Down), exploration.Location);
    }

    [Fact]
    public void StartNew_TestWorld_DoorLeadsIntoInterior()
    {
        // Below the door at (5.36, 13.14), facing it: open it, stand still, then walk in.
        var catalog = ContentCatalogLoader.Load(_testWorldRoot);
        var belowDoor = new MapLocation("start", new Point2D(5.86, 14.6), Facing.Up);
        var exploration = GameSession.StartNew(catalog with { NewGame = catalog.NewGame with { StartLocation = belowDoor } }, "sarxian", new SeededRandom(0)).Exploration;

        var opened = exploration.TryInteract();
        exploration.Walk(MoveIntent.None, TimeSpan.FromSeconds(0.1));
        exploration.Walk(MoveIntent.FromAxes(0, -1), TimeSpan.FromSeconds(0.05));
        var stepAtDoor = exploration.CurrentEventStep;
        exploration.ContinueEvent();

        Assert.True(opened);
        Assert.IsType<WarpStep>(stepAtDoor);
        Assert.Equal("interior_1", exploration.Location.MapId);
        Assert.False(exploration.IsEventRunning);
    }
}
