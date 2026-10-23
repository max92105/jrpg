using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>Touch events started by walking on <see cref="TestExplorationMap"/>.</summary>
public sealed class ExplorationTouchEventTests
{
    private readonly Inventory _inventory = new();

    [Fact]
    public void Walk_EnterCellWithTouchEvent_StartsIt()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 3.5), Facing.Right);

        exploration.Walk(MoveIntent.FromAxes(1, 0), TestExplorationMap.ThirdOfASecond);

        Assert.Equal(new GridCell(1, 3), GridCell.Containing(exploration.Location.Position));
        Assert.Equal(TestExplorationMap.Greeting, exploration.CurrentEventStep);
    }

    [Fact]
    public void Walk_WithinSameCell_DoesNotRestartCellEvent()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 3.5), Facing.Right);
        exploration.Walk(MoveIntent.FromAxes(1, 0), TestExplorationMap.ThirdOfASecond);
        exploration.ContinueEvent();

        exploration.Walk(MoveIntent.FromAxes(0, -0.5), TimeSpan.FromSeconds(0.1));

        Assert.Equal(new GridCell(1, 3), GridCell.Containing(exploration.Location.Position));
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_ArrivalCell_StaysQuiet()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(1.2, 3.5), Facing.Right);

        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(0.1));

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_IntoObjectBeforeStandingStill_DoesNotStartTouchEvent()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(3.5, 2.5), Facing.Right);

        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(0.01));

        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_KeepPushingIntoObjectAfterTouchEvent_DoesNotRefireUntilStopped()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(3.5, 2.5), Facing.Right);
        var pushRight = MoveIntent.FromAxes(1, 0);
        var frame = TimeSpan.FromSeconds(0.01);
        exploration.Walk(MoveIntent.None, frame);
        exploration.Walk(pushRight, frame);
        Assert.Equal(TestExplorationMap.Greeting, exploration.CurrentEventStep);
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
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 1.5), Facing.Down);
        exploration.Walk(MoveIntent.FromAxes(0, 1), TestExplorationMap.ThirdOfASecond);
        Assert.Equal(TestExplorationMap.Greeting, exploration.CurrentEventStep);
        exploration.ContinueEvent();
        exploration.Walk(MoveIntent.FromAxes(0, -1), TestExplorationMap.ThirdOfASecond);

        exploration.Walk(MoveIntent.FromAxes(0, 1), TestExplorationMap.ThirdOfASecond);

        Assert.Equal(new GridCell(0, 2), GridCell.Containing(exploration.Location.Position));
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void Walk_PushOnIntoObjectAfterCellEvent_WaitsUntilStopped()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 2.5), Facing.Right);
        exploration.Walk(MoveIntent.None, TimeSpan.FromSeconds(0.01));
        exploration.Walk(MoveIntent.FromAxes(1, 0), TestExplorationMap.ThirdOfASecond);
        Assert.Equal(new GridCell(3, 2), GridCell.Containing(exploration.Location.Position));
        exploration.ContinueEvent();

        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(0.01));

        Assert.False(exploration.IsEventRunning);
    }
}
