using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>
/// An NPC faces the player for the whole of its own event (ADR 0044) and turns back after: the notice board of
/// <see cref="TestExplorationMap"/> at (4, 0), talked to from its left, and a shopkeeper whose shop stays open.
/// </summary>
public sealed class ExplorationNpcFacingTests
{
    private static readonly ObjectDefinition _counter = new()
    {
        Id = "counter",
        Width = 1,
        Height = 1,
        CollisionPolygon = [],
        Category = "test",
        Tiles = [],
        AbovePlayerZones = [],
    };

    private static PlacedObject Npc(string uid, Point2D position, params MapEvent[] events) =>
        new() { Uid = uid, ObjectId = "counter", Position = position, Z = MapLayers.PlayerZ, Events = events };

    [Fact]
    public void NpcFacingOf_TalkedToFromItsLeft_FacesLeftThenDownAfter()
    {
        var exploration = TestExplorationMap.Start(new Inventory(), new Point2D(3.5, 0.5), Facing.Right);
        var board = exploration.CurrentMap.Layout.Objects.Single(placed => placed.Uid == "notice@4,0");
        Assert.Equal(Facing.Down, exploration.NpcFacingOf(board));

        Assert.True(exploration.TryInteract());
        Assert.Equal(Facing.Left, exploration.NpcFacingOf(board));

        exploration.ContinueEvent();
        Assert.False(exploration.IsEventRunning);
        Assert.Equal(Facing.Down, exploration.NpcFacingOf(board));
    }

    [Fact]
    public void NpcFacingOf_CellEventRunning_StaysDown()
    {
        var exploration = TestExplorationMap.Start(new Inventory(), new Point2D(1.5, 2.5), Facing.Down);
        var board = exploration.CurrentMap.Layout.Objects.Single(placed => placed.Uid == "notice@4,0");

        exploration.Walk(MoveIntent.FromAxes(0, 1), TestExplorationMap.ThirdOfASecond);

        Assert.True(exploration.IsEventRunning);
        Assert.Equal(Facing.Down, exploration.NpcFacingOf(board));
    }

    // An event that warps mid-way keeps running on the next map; a same-named object there is not its source.
    [Fact]
    public void NpcFacingOf_EventWarpedOntoAMapWithTheSameUid_LeavesThatNpcDown()
    {
        var greeting = new DialogueStep(new Speech(["Follow me."]));
        var warp = new WarpStep("street", new GridCell(1, 1));
        var lead = new MapEventBuilder().WithId("villager_0").WithSteps(greeting, warp, greeting).Build();
        var inn = new MapLayoutBuilder().WithLayer("111", "111").Build() with
        {
            Id = "inn",
            Objects = [Npc("villager_1", new Point2D(1, 0), lead)],
        };
        var street = new MapLayoutBuilder().WithLayer("111", "111").Build() with
        {
            Id = "street",
            Objects = [Npc("villager_1", new Point2D(2, 0))],
        };
        var catalog = new ContentCatalogBuilder()
            .WithTiles(TestTiles.Walkable(1))
            .WithObjects(_counter)
            .WithMaps(inn, street)
            .Build();
        var context = new ExplorationContext(catalog, new Inventory(), new WorldState(), new FixedSequenceRandom());
        var start = new MapLocation("inn", new Point2D(2.5, 0.5), Facing.Left);
        var exploration = new ExplorationState(context, TestParties.LedBy("sarxian"), start);
        Assert.True(exploration.TryInteract());
        Assert.Equal(Facing.Right, exploration.NpcFacingOf(inn.Objects[0]));

        exploration.ContinueEvent();
        exploration.ContinueEvent();

        Assert.Equal("street", exploration.CurrentMap.Id);
        Assert.Same(greeting, exploration.CurrentEventStep);
        Assert.Equal(Facing.Down, exploration.NpcFacingOf(exploration.CurrentMap.Layout.Objects[0]));
    }

    [Fact]
    public void NpcFacingOf_ShopOpenedByItsEvent_FacesThePlayerUntilClosed()
    {
        var sell = new MapEventBuilder().WithId("counter_0").WithSteps(new OpenShopStep("general_store")).Build();
        var layout = new MapLayoutBuilder().WithLayer("111", "111").Build() with
        {
            Objects =
            [
                new PlacedObject
                {
                    Uid = "counter@1,0",
                    ObjectId = "counter",
                    Position = new Point2D(1, 0),
                    Z = MapLayers.PlayerZ,
                    Events = [sell],
                },
            ],
        };
        var catalog = new ContentCatalogBuilder().WithTiles(TestTiles.Walkable(1)).WithObjects(_counter).WithMaps(layout).Build();
        var context = new ExplorationContext(catalog, new Inventory(), new WorldState(), new FixedSequenceRandom());
        var start = new MapLocation(layout.Id, new Point2D(2.5, 0.5), Facing.Left);
        var exploration = new ExplorationState(context, TestParties.LedBy("sarxian"), start);
        var keeper = layout.Objects[0];

        Assert.True(exploration.TryInteract());
        Assert.IsType<OpenShopStep>(exploration.CurrentEventStep);
        Assert.Equal(Facing.Right, exploration.NpcFacingOf(keeper));

        exploration.ContinueEvent();

        Assert.False(exploration.IsEventRunning);
        Assert.Equal(Facing.Down, exploration.NpcFacingOf(keeper));
    }
}
