using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Session;

public sealed class RepositoryNewGameTests
{
    [Fact]
    public void StartNew_RepositoryContent_LeaderStartsAlone()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(0));

        Assert.Equal("sarxian", session.Party.Leader.Id);
        Assert.Equal(["sarxian"], session.Party.Members.Select(member => member.Id));
        Assert.Equal(["sarxian"], session.Party.ActiveMembers.Select(member => member.Id));
        Assert.Equal(5, session.Inventory.QuantityOf("potion"));
    }

    [Fact]
    public void StartNew_RepositoryContent_EveryLeaderChoiceStarts()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var parties = catalog.NewGame.LeaderChoices.Select(choice => GameSession.StartNew(catalog, choice.MemberId, new SeededRandom(0)).Party);

        Assert.Equal(
            catalog.NewGame.LeaderChoices.Select(choice => new[] { choice.MemberId }),
            parties.Select(party => party.Members.Select(member => member.Id).ToArray()));
    }

    [Fact]
    public void StartNew_RepositoryContent_StartsOnStartMap()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(0));

        Assert.Equal(new MapLocation("start", new Point2D(10, 3), Facing.Down), session.Exploration.Location);
    }

    [Fact]
    public void StartNew_RepositoryContent_DoorLeadsIntoInterior()
    {
        // Below the door at (5.36, 13.14), facing it: open it, stand still, then walk in.
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
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
