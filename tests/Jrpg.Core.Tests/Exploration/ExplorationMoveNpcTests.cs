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
/// A <c>moveNpc</c> step walks an NPC while the event waits (ADR 0044): a 5 x 4 "hall" with a villager at (3, 1) whose
/// talk walks it two cells left and one down before its line, and a "yard" to warp to and back from.
/// </summary>
public sealed class ExplorationMoveNpcTests
{
    private static readonly ObjectDefinition _villager = new()
    {
        Id = "villager",
        Width = 1,
        Height = 1,
        CollisionPolygon = [],
        Category = "test",
        Tiles = [],
        FieldSpriteFile = "npc_placeholder.png",
        AbovePlayerZones = [],
    };

    private static readonly DialogueStep _greeting = new(new Speech(["Come in."]));
    private static readonly MoveNpcStep _walkOver = new("villager_1", [Facing.Left, Facing.Left, Facing.Down]);
    private static readonly StillStep _still = new("Placeholder1.png", 4, Caption: null);

    [Fact]
    public void AdvanceNpcWalks_UntilArrived_ContinuesTheEvent()
    {
        var exploration = Start(new MapEventBuilder().WithId("villager_0").WithSteps(_walkOver, _greeting).Build());
        Assert.True(exploration.TryInteract());
        Assert.Same(_walkOver, exploration.CurrentEventStep);

        exploration.AdvanceNpcWalks(TimeSpan.FromSeconds(0.5));
        Assert.Same(_walkOver, exploration.CurrentEventStep);
        exploration.AdvanceNpcWalks(TimeSpan.FromSeconds(5));

        Assert.Same(_greeting, exploration.CurrentEventStep);
    }

    [Fact]
    public void NpcPoseOf_DuringWalk_FacesTheStepAndMoves()
    {
        var exploration = Start(new MapEventBuilder().WithId("villager_0").WithSteps(_walkOver, _greeting).Build());
        exploration.TryInteract();

        exploration.AdvanceNpcWalks(TimeSpan.FromSeconds(0.5));

        var pose = exploration.NpcPoseOf(Villager(exploration));
        Assert.True(pose.IsWalking);
        Assert.Equal(Facing.Left, pose.Facing);
        Assert.InRange(pose.Position.X, 1, 2.99);
        Assert.Equal(TimeSpan.FromSeconds(0.5), pose.Walked);
    }

    [Fact]
    public void NpcPoseOf_AfterWalk_StaysAtEndFacingLastStep()
    {
        var exploration = Start(new MapEventBuilder().WithId("villager_0").WithSteps(_walkOver, _greeting).Build());
        exploration.TryInteract();
        exploration.AdvanceNpcWalks(TimeSpan.FromSeconds(5));
        exploration.ContinueEvent();
        Assert.False(exploration.IsEventRunning);

        var pose = exploration.NpcPoseOf(Villager(exploration));

        Assert.Equal((new Point2D(1, 2), Facing.Down, false), (pose.Position, pose.Facing, pose.IsWalking));
    }

    [Fact]
    public void NpcPoseOf_AfterLeavingAndComingBack_IsThePlacedOne()
    {
        var leave = new WarpStep("yard", new GridCell(0, 0));
        var walkThenLeave = new MapEventBuilder().WithId("villager_0").WithSteps(_walkOver, leave).Build();
        var exploration = Start(walkThenLeave);
        exploration.TryInteract();
        exploration.AdvanceNpcWalks(TimeSpan.FromSeconds(5));
        exploration.ContinueEvent();
        Assert.Equal("yard", exploration.CurrentMap.Id);

        exploration.Walk(MoveIntent.FromAxes(1, 0), TestExplorationMap.ThirdOfASecond);
        exploration.ContinueEvent();
        Assert.Equal("hall", exploration.CurrentMap.Id);

        var pose = exploration.NpcPoseOf(Villager(exploration));
        Assert.Equal((new Point2D(3, 1), Facing.Down), (pose.Position, pose.Facing));
    }

    [Fact]
    public void SkipEventPresentation_StillThenMove_PlacesNpcAtPathEnd()
    {
        var exploration = Start(new MapEventBuilder().WithId("villager_0").WithSteps(_still, _walkOver, _greeting).Build());
        exploration.TryInteract();
        Assert.Same(_still, exploration.CurrentEventStep);

        exploration.SkipEventPresentation();

        Assert.Same(_greeting, exploration.CurrentEventStep);
        Assert.Equal(new Point2D(1, 2), exploration.NpcPoseOf(Villager(exploration)).Position);
    }

    // Nothing advances the walks during a skip: the second move must still start from the first's end (decision 578).
    [Fact]
    public void SkipEventPresentation_StillThenTwoMovesOfOneNpc_PlacesNpcAtSecondPathEnd()
    {
        var stepLeft = new MoveNpcStep("villager_1", [Facing.Left]);
        var stepDown = new MoveNpcStep("villager_1", [Facing.Down]);
        var twoMoves = new MapEventBuilder().WithId("villager_0").WithSteps(_still, stepLeft, stepDown, _greeting).Build();
        var exploration = Start(twoMoves);
        exploration.TryInteract();

        exploration.SkipEventPresentation();

        Assert.Same(_greeting, exploration.CurrentEventStep);
        var pose = exploration.NpcPoseOf(Villager(exploration));
        Assert.Equal((new Point2D(2, 2), Facing.Down), (pose.Position, pose.Facing));
    }

    [Fact]
    public void Walk_WhileNpcWalks_Throws()
    {
        var exploration = Start(new MapEventBuilder().WithId("villager_0").WithSteps(_walkOver, _greeting).Build());
        exploration.TryInteract();

        var down = MoveIntent.FromAxes(0, 1);

        Assert.Throws<InvalidOperationException>(() => exploration.Walk(down, TestExplorationMap.ThirdOfASecond));
    }

    private static PlacedObject Villager(ExplorationState exploration) =>
        exploration.CurrentMap.Layout.Objects.Single(placed => placed.Uid == "villager_1");

    // The player stands under the villager, facing up at it; the yard's cell (1, 0) warps back to the hall's (0, 3).
    private static ExplorationState Start(MapEvent talk)
    {
        var hall = new MapLayoutBuilder().WithLayer("11111", "11111", "11111", "11111").Build() with
        {
            Id = "hall",
            Objects =
            [
                new PlacedObject
                {
                    Uid = "villager_1",
                    ObjectId = "villager",
                    Position = new Point2D(3, 1),
                    Z = MapLayers.PlayerZ,
                    Events = [talk],
                },
            ],
        };
        var back = new MapEventBuilder().WithId("cell_1_0").AsTouch().WithSteps(new WarpStep("hall", new GridCell(0, 3))).Build();
        var yard = new MapLayoutBuilder().WithLayer("111", "111").Build() with
        {
            Id = "yard",
            CellEvents = [new CellEvent(new GridCell(1, 0), back)],
        };
        var catalog = new ContentCatalogBuilder()
            .WithTiles(TestTiles.Walkable(1))
            .WithObjects(_villager)
            .WithMaps(hall, yard)
            .Build();
        var context = new ExplorationContext(catalog, new Inventory(), new WorldState(), new FixedSequenceRandom());
        var start = new MapLocation("hall", new Point2D(3.5, 2.5), Facing.Up);
        return new ExplorationState(context, TestParties.LedBy("sarxian"), start);
    }
}
