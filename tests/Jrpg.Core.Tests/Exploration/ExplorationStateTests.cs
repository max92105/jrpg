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
/// A 5x5 floor: a chest at (2, 0) that gives a potion once and opens, a lever at (0, 0) that only swaps, an open door
/// at (4, 2) that greets on touch with a doormat cell before it at (3, 2), a sign cell at (1, 3), and a cell at (0, 2)
/// that talks only once.
/// </summary>
public sealed class ExplorationStateTests
{
    private static readonly TimeSpan _thirdOfASecond = TimeSpan.FromSeconds(1.0 / 3);

    private static readonly ObjectDefinition _chest = new() { Id = "chest", Width = 1, Height = 1, CollisionPolygon = [], Category = "test", Tiles = [], AbovePlayerZones = [] };
    private static readonly ObjectDefinition _openedChest = _chest with { Id = "chest_opened" };
    private static readonly ObjectDefinition _lever = _chest with { Id = "lever" };
    private static readonly ObjectDefinition _leverPulled = _chest with { Id = "lever_pulled" };
    private static readonly ObjectDefinition _openDoor = _chest with { Id = "door_open" };
    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").Build();

    private static readonly DialogueStep _greeting = new(new Speech(["Welcome!"]));
    private static readonly MapEvent _openChest = new MapEventBuilder().WithId("chest_0").AsOnce()
        .WithSteps(new GiveItemStep("potion", 1), new SwapObjectStep("chest_opened")).Build();

    private static readonly MapEvent _pullLever = new MapEventBuilder().WithId("lever_0").WithSteps(new SwapObjectStep("lever_pulled")).Build();
    private static readonly MapEvent _greetAtDoor = new MapEventBuilder().WithId("door_0").AsTouch().WithSteps(_greeting).Build();
    private static readonly MapEvent _readSign = new MapEventBuilder().WithId("cell_1_3").AsTouch().WithSteps(_greeting).Build();
    private static readonly MapEvent _rememberOnce = new MapEventBuilder().WithId("cell_0_2").AsTouch().AsOnce().WithSteps(_greeting).Build();
    private static readonly MapEvent _wipeFeet = new MapEventBuilder().WithId("cell_3_2").AsTouch().WithSteps(_greeting).Build();

    private readonly Inventory _inventory = new();

    [Fact]
    public void TryInteract_NothingInFront_ReturnsFalse()
    {
        var exploration = StartAt(new Point2D(0.5, 4.5), Facing.Down);

        Assert.False(exploration.TryInteract());
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void TryInteract_SwapOnlyEvent_LeavesNoEventRunning()
    {
        var exploration = StartAt(new Point2D(0.5, 1.5), Facing.Up);

        var started = exploration.TryInteract();

        Assert.True(started);
        Assert.False(exploration.IsEventRunning);
        Assert.Null(exploration.CurrentEventStep);
    }

    [Fact]
    public void ContinueEvent_ChestEvent_AddsPotionAndOpensChest()
    {
        var exploration = StartAt(new Point2D(2.5, 1.5), Facing.Up);

        exploration.TryInteract();
        Assert.Equal(new GiveItemStep("potion", 1), exploration.CurrentEventStep);
        exploration.ContinueEvent();

        Assert.Equal(1, _inventory.QuantityOf("potion"));
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void TryInteract_OpenedChest_ReturnsFalse()
    {
        var exploration = StartAt(new Point2D(2.5, 1.5), Facing.Up);
        exploration.TryInteract();
        exploration.ContinueEvent();

        var startedAgain = exploration.TryInteract();

        Assert.False(startedAgain);
        Assert.Equal(1, _inventory.QuantityOf("potion"));
    }

    [Fact]
    public void Walk_EnterCellWithTouchEvent_StartsIt()
    {
        var exploration = StartAt(new Point2D(0.5, 3.5), Facing.Right);

        exploration.Walk(MoveIntent.FromAxes(1, 0), _thirdOfASecond);

        Assert.Equal(new GridCell(1, 3), GridCell.Containing(exploration.Location.Position));
        Assert.Equal(_greeting, exploration.CurrentEventStep);
    }

    [Fact]
    public void Walk_WithinSameCell_DoesNotRestartCellEvent()
    {
        var exploration = StartAt(new Point2D(0.5, 3.5), Facing.Right);
        exploration.Walk(MoveIntent.FromAxes(1, 0), _thirdOfASecond);
        exploration.ContinueEvent();

        exploration.Walk(MoveIntent.FromAxes(0, -0.5), TimeSpan.FromSeconds(0.1));

        Assert.Equal(new GridCell(1, 3), GridCell.Containing(exploration.Location.Position));
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_ArrivalCell_StaysQuiet()
    {
        var exploration = StartAt(new Point2D(1.2, 3.5), Facing.Right);

        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(0.1));

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_IntoObjectBeforeStandingStill_DoesNotStartTouchEvent()
    {
        var exploration = StartAt(new Point2D(3.5, 2.5), Facing.Right);

        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(0.01));

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_KeepPushingIntoObjectAfterTouchEvent_DoesNotRefireUntilStopped()
    {
        var exploration = StartAt(new Point2D(3.5, 2.5), Facing.Right);
        var pushRight = MoveIntent.FromAxes(1, 0);
        var frame = TimeSpan.FromSeconds(0.01);
        exploration.Walk(MoveIntent.None, frame);
        exploration.Walk(pushRight, frame);
        Assert.Equal(_greeting, exploration.CurrentEventStep);
        exploration.ContinueEvent();

        exploration.Walk(pushRight, frame);
        var refiredWhilePushing = exploration.IsEventRunning;
        exploration.Walk(MoveIntent.None, frame);
        exploration.Walk(pushRight, frame);

        Assert.False(refiredWhilePushing);
        Assert.True(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_ReenterDoneOnceCellEvent_DoesNotStartIt()
    {
        var exploration = StartAt(new Point2D(0.5, 1.5), Facing.Down);
        exploration.Walk(MoveIntent.FromAxes(0, 1), _thirdOfASecond);
        Assert.Equal(_greeting, exploration.CurrentEventStep);
        exploration.ContinueEvent();
        exploration.Walk(MoveIntent.FromAxes(0, -1), _thirdOfASecond);

        exploration.Walk(MoveIntent.FromAxes(0, 1), _thirdOfASecond);

        Assert.Equal(new GridCell(0, 2), GridCell.Containing(exploration.Location.Position));
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_PushOnIntoObjectAfterCellEvent_WaitsUntilStopped()
    {
        var exploration = StartAt(new Point2D(2.5, 2.5), Facing.Right);
        exploration.Walk(MoveIntent.None, TimeSpan.FromSeconds(0.01));
        exploration.Walk(MoveIntent.FromAxes(1, 0), _thirdOfASecond);
        Assert.Equal(new GridCell(3, 2), GridCell.Containing(exploration.Location.Position));
        exploration.ContinueEvent();

        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(0.01));

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void TryInteract_WhileEventRunning_Throws()
    {
        var exploration = StartAt(new Point2D(2.5, 1.5), Facing.Up);
        exploration.TryInteract();

        Assert.Throws<InvalidOperationException>(() => exploration.TryInteract());
    }

    [Fact]
    public void Walk_WhileEventRunning_Throws()
    {
        var exploration = StartAt(new Point2D(2.5, 1.5), Facing.Up);
        exploration.TryInteract();

        Assert.Throws<InvalidOperationException>(() => exploration.Walk(MoveIntent.FromAxes(1, 0), _thirdOfASecond));
    }

    [Fact]
    public void ContinueEvent_NoEventRunning_Throws()
    {
        var exploration = StartAt(new Point2D(0.5, 4.5), Facing.Down);

        Assert.Throws<InvalidOperationException>(exploration.ContinueEvent);
    }

    private ExplorationState StartAt(Point2D position, Facing facing)
    {
        var layout = new MapLayoutBuilder().WithLayer("11111", "11111", "11111", "11111", "11111").Build() with
        {
            Objects =
            [
                new PlacedObject { Uid = "lever@0,0", ObjectId = "lever", Position = new Point2D(0, 0), Events = [_pullLever] },
                new PlacedObject { Uid = "chest@2,0", ObjectId = "chest", Position = new Point2D(2, 0), Events = [_openChest] },
                new PlacedObject { Uid = "door@4,2", ObjectId = "door_open", Position = new Point2D(4, 2), Events = [_greetAtDoor] },
            ],
            CellEvents =
            [
                new CellEvent(new GridCell(1, 3), _readSign),
                new CellEvent(new GridCell(0, 2), _rememberOnce),
                new CellEvent(new GridCell(3, 2), _wipeFeet),
            ],
        };
        var catalog = new ContentCatalogBuilder()
            .WithTiles(TestTiles.Walkable(1))
            .WithObjects(_chest, _openedChest, _lever, _leverPulled, _openDoor)
            .WithItems(_potion)
            .WithMaps(layout)
            .Build();
        var context = new ExplorationContext(catalog, _inventory, new WorldState(), new FixedSequenceRandom());
        return new ExplorationState(context, new MapLocation(layout.Id, position, facing));
    }
}
