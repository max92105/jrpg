using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>What the player could interact with (<c>InteractionInFront</c>) on <see cref="TestExplorationMap"/>.</summary>
public sealed class ExplorationInteractionTests
{
    private readonly Inventory _inventory = new();

    [Fact]
    public void InteractionInFront_FacingChest_ReturnsItsInteractEvent()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);

        var target = exploration.InteractionInFront;

        Assert.Equal("chest@2,0", target?.Placed.Uid);
        Assert.Same(TestExplorationMap.OpenChest, target?.Event);
    }

    [Fact]
    public void InteractionInFront_FacingAway_IsNull()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Down);

        Assert.Null(exploration.InteractionInFront);
    }

    [Fact]
    public void InteractionInFront_WhileEventRuns_IsNull()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);

        exploration.TryInteract();

        Assert.Null(exploration.InteractionInFront);
    }

    [Fact]
    public void InteractionInFront_OnceEventDone_IsNull()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);
        exploration.TryInteract();

        exploration.ContinueEvent();

        Assert.Null(exploration.InteractionInFront);
    }

    [Fact]
    public void InteractionInFront_TouchEventOnly_IsNull()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(3.5, 2.5), Facing.Right);

        Assert.Null(exploration.InteractionInFront);
    }

    [Fact]
    public void InteractionInFront_WalkingTowardSameObject_KeepsTheSameRecord()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.6), Facing.Up);
        var step = TimeSpan.FromMilliseconds(10);
        exploration.Walk(MoveIntent.FromAxes(0, -1), step);
        var first = exploration.InteractionInFront;

        exploration.Walk(MoveIntent.FromAxes(0, -1), step);

        Assert.NotNull(first);
        Assert.Same(first, exploration.InteractionInFront);
    }

    [Fact]
    public void InteractionInFront_InteractEventFinishingAtOnce_IsRefreshed()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 1.5), Facing.Up);

        exploration.TryInteract();

        Assert.Equal("lever_pulled", exploration.CurrentDefinitionOf(exploration.InteractionInFront!.Placed).Id);
        Assert.Same(TestExplorationMap.PullLever, exploration.InteractionInFront.Event);
    }

    [Fact]
    public void InteractionInFront_RepeatableTalkEnded_IsSetAgain()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(4.5, 1.5), Facing.Up);
        exploration.TryInteract();

        exploration.ContinueEvent();

        Assert.Equal("notice@4,0", exploration.InteractionInFront?.Placed.Uid);
    }
}
