using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>Interacting, events and walking on <see cref="TestExplorationMap"/>.</summary>
public sealed class ExplorationStateTests
{
    private readonly Inventory _inventory = new();

    [Fact]
    public void TryInteract_NothingInFront_ReturnsFalse()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 4.5), Facing.Down);

        Assert.False(exploration.TryInteract());
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void TryInteract_SwapOnlyEvent_LeavesNoEventRunning()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 1.5), Facing.Up);

        var started = exploration.TryInteract();

        Assert.True(started);
        Assert.False(exploration.IsEventRunning);
        Assert.Null(exploration.CurrentEventStep);
    }

    [Fact]
    public void ContinueEvent_ChestEvent_AddsPotionAndOpensChest()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);

        exploration.TryInteract();
        Assert.Equal(new GiveItemStep("potion", 1), exploration.CurrentEventStep);
        exploration.ContinueEvent();

        Assert.Equal(1, _inventory.QuantityOf("potion"));
        Assert.False(exploration.IsEventRunning);
    }

    [Fact]
    public void TryInteract_OpenedChest_ReturnsFalse()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);
        exploration.TryInteract();
        exploration.ContinueEvent();

        var startedAgain = exploration.TryInteract();

        Assert.False(startedAgain);
        Assert.Equal(1, _inventory.QuantityOf("potion"));
    }

    [Fact]
    public void CurrentItemReceipt_OnGiveItemStep_ReportsAddedCopies()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);

        exploration.TryInteract();

        Assert.Equal(new ItemReceipt(TestExplorationMap.Potion, Added: 1, Lost: 0), exploration.CurrentItemReceipt);
    }

    [Fact]
    public void CurrentItemReceipt_AfterContinue_IsNull()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);
        exploration.TryInteract();

        exploration.ContinueEvent();

        Assert.Null(exploration.CurrentItemReceipt);
    }

    [Fact]
    public void CurrentDefinitionOf_AfterSwap_ReturnsSwappedDefinition()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 1.5), Facing.Up);
        var lever = exploration.CurrentMap.Layout.Objects.Single(placed => placed.ObjectId == "lever");

        exploration.TryInteract();

        Assert.Equal(TestExplorationMap.LeverPulled, exploration.CurrentDefinitionOf(lever));
    }

    [Fact]
    public void CurrentDefinitionOf_NeverSwapped_ReturnsPlacedDefinition()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 1.5), Facing.Up);
        var lever = exploration.CurrentMap.Layout.Objects.Single(placed => placed.ObjectId == "lever");

        var definition = exploration.CurrentDefinitionOf(lever);

        Assert.Equal(TestExplorationMap.Lever, definition);
    }

    [Fact]
    public void TryInteract_WhileEventRunning_Throws()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);
        exploration.TryInteract();

        Assert.Throws<InvalidOperationException>(() => exploration.TryInteract());
    }

    [Fact]
    public void Walk_WhileEventRunning_Throws()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(2.5, 1.5), Facing.Up);
        exploration.TryInteract();

        Assert.Throws<InvalidOperationException>(() => exploration.Walk(MoveIntent.FromAxes(1, 0), TestExplorationMap.ThirdOfASecond));
    }

    [Fact]
    public void ContinueEvent_NoEventRunning_Throws()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 4.5), Facing.Down);

        Assert.Throws<InvalidOperationException>(exploration.ContinueEvent);
    }

    [Fact]
    public void IsPlayerMoving_PushingIntoMapEdge_IsTrue()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(4.5, 4.5), Facing.Down);
        exploration.Walk(MoveIntent.FromAxes(0, 1), TestExplorationMap.ThirdOfASecond);
        var againstEdge = exploration.PlayerPosition;

        exploration.Walk(MoveIntent.FromAxes(0, 1), TestExplorationMap.ThirdOfASecond);

        Assert.Equal(againstEdge, exploration.PlayerPosition);
        Assert.True(exploration.IsPlayerMoving);
    }

    [Fact]
    public void IsPlayerMoving_NoIntent_IsFalse()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 4.5), Facing.Down);
        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(0.1));

        exploration.Walk(MoveIntent.None, TimeSpan.FromSeconds(0.1));

        Assert.False(exploration.IsPlayerMoving);
    }

    [Fact]
    public void PlayerPosition_AfterWalking_MatchesLocation()
    {
        var exploration = TestExplorationMap.Start(_inventory, new Point2D(0.5, 4.5), Facing.Down);

        exploration.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(0.1));

        Assert.Equal(exploration.Location.Position, exploration.PlayerPosition);
        Assert.Equal(Facing.Right, exploration.PlayerFacing);
    }
}
