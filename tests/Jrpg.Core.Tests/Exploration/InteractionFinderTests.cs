using Jrpg.Core.Content;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Exploration;

public sealed class InteractionFinderTests
{
    private static readonly ExplorationRules _rules = new() { WalkSpeed = 3, PlayerRadius = 0.1, InteractReach = 0.6 };
    private static readonly ObjectDefinition _chest = new() { Id = "chest", Width = 1, Height = 1, CollisionPolygon = [] };
    private static readonly DialogueStep _look = new(new Speech(["A chest."]));

    // The player stands below the cell (2, 1) and faces up, so the probe lands at (2.5, 1.9).
    private static readonly Point2D _belowChest = new(2.5, 2.5);

    [Fact]
    public void FindInFront_FacingChest_ReturnsItsInteractEvent()
    {
        var openChest = new MapEventBuilder().WithSteps(_look).Build();
        var map = MapWith(_chest, openChest);

        var triggered = new InteractionFinder(map, _rules).FindInFront(_belowChest, Facing.Up, EventActivation.Interact);

        Assert.Equal(openChest, triggered?.Event);
        Assert.Equal(map.Definition.Layout.Objects[0], triggered?.Source);
    }

    [Fact]
    public void FindInFront_FacingAway_ReturnsNull()
    {
        var map = MapWith(_chest, new MapEventBuilder().WithSteps(_look).Build());

        var triggered = new InteractionFinder(map, _rules).FindInFront(_belowChest, Facing.Down, EventActivation.Interact);

        Assert.Null(triggered);
    }

    [Fact]
    public void FindInFront_TwoOverlappingObjects_ReturnsLastPlaced()
    {
        var below = new MapEventBuilder().WithId("below_0").WithSteps(_look).Build();
        var onTop = new MapEventBuilder().WithId("on_top_0").WithSteps(_look).Build();
        var map = new ActiveMapBuilder().WithRows("111", "111", "111")
            .WithObject(_chest, new Point2D(2, 1), below)
            .WithObject(_chest, new Point2D(2, 1), onTop)
            .Build();

        var triggered = new InteractionFinder(map, _rules).FindInFront(_belowChest, Facing.Up, EventActivation.Interact);

        Assert.Equal("on_top_0", triggered?.Event.Id);
    }

    [Fact]
    public void FindInFront_OnceEventDone_ReturnsNextEventOnObject()
    {
        var firstVisit = new MapEventBuilder().WithId("chest_0").AsOnce().WithSteps(_look).Build();
        var laterVisits = new MapEventBuilder().WithId("chest_1").WithSteps(_look).Build();
        var map = MapWith(_chest, firstVisit, laterVisits);
        map.MarkDone(firstVisit);

        var triggered = new InteractionFinder(map, _rules).FindInFront(_belowChest, Facing.Up, EventActivation.Interact);

        Assert.Equal("chest_1", triggered?.Event.Id);
    }

    [Fact]
    public void FindInFront_OnlyWhenObjectNotCurrent_ReturnsNull()
    {
        var closedDoor = _chest with { Id = "door_close" };
        var enterOpenDoor = new MapEventBuilder().AsTouch().OnlyWhenObjectIs("door_open").WithSteps(_look).Build();
        var map = MapWith(closedDoor, enterOpenDoor);

        var triggered = new InteractionFinder(map, _rules).FindInFront(_belowChest, Facing.Up, EventActivation.Touch);

        Assert.Null(triggered);
    }

    private static ActiveMap MapWith(ObjectDefinition definition, params MapEvent[] events) =>
        new ActiveMapBuilder().WithRows("111", "111", "111").WithObject(definition, new Point2D(2, 1), events).Build();
}
