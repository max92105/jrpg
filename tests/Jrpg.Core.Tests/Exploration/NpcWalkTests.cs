using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Exploration;

/// <summary>One NPC walking a path (ADR 0044) on a 4 x 3 map, from the cell (1, 1), at 2 cells a second.</summary>
public sealed class NpcWalkTests
{
    private const double Speed = 2;

    private static readonly MapLayout _layout = new MapLayoutBuilder().WithLayer("1111", "1111", "1111").Build();
    private static readonly Point2D _start = new(1, 1);

    // Only the step off the map goes (decision 578): the one after it walks on from where the path stood.
    [Fact]
    public void Start_StepLeavingTheMap_IsDropped()
    {
        var walk = new NpcWalk(_start, Facing.Down, [Facing.Up, Facing.Up, Facing.Right], _layout);

        Assert.Equal((new Point2D(2, 0), Facing.Right), walk.End);
    }

    [Fact]
    public void Advance_HalfACell_IsBetweenCells()
    {
        var walk = new NpcWalk(_start, Facing.Down, [Facing.Right], _layout);

        walk.Advance(TimeSpan.FromMilliseconds(250), Speed);

        Assert.Equal(new Point2D(1.5, 1), walk.Position);
        Assert.False(walk.IsFinished);
    }

    [Fact]
    public void Advance_PastTheWholePath_StopsOnTheLastCell()
    {
        var walk = new NpcWalk(_start, Facing.Down, [Facing.Right, Facing.Down], _layout);

        walk.Advance(TimeSpan.FromSeconds(5), Speed);

        Assert.Equal(new Point2D(2, 2), walk.Position);
        Assert.True(walk.IsFinished);
    }

    [Fact]
    public void Facing_SecondStep_IsItsDirection()
    {
        var walk = new NpcWalk(_start, Facing.Down, [Facing.Right, Facing.Up], _layout);

        walk.Advance(TimeSpan.FromMilliseconds(600), Speed);

        Assert.Equal(Facing.Up, walk.Facing);
        Assert.Equal(new Point2D(2, 0.8), walk.Position);
    }

    [Fact]
    public void IsFinished_AfterTheLastCell_IsTrue()
    {
        var walk = new NpcWalk(_start, Facing.Down, [Facing.Left], _layout);

        walk.Advance(TimeSpan.FromMilliseconds(500), Speed);

        Assert.True(walk.IsFinished);
        Assert.Equal(Facing.Left, walk.Facing);
    }

    [Fact]
    public void Advance_ZeroDelta_Stays()
    {
        var walk = new NpcWalk(_start, Facing.Down, [Facing.Left], _layout);

        walk.Advance(TimeSpan.Zero, Speed);

        Assert.Equal((_start, TimeSpan.Zero), (walk.Position, walk.Walked));
    }

    [Fact]
    public void Finish_MidWalk_JumpsToTheEnd()
    {
        var walk = new NpcWalk(_start, Facing.Down, [Facing.Right, Facing.Right], _layout);
        walk.Advance(TimeSpan.FromMilliseconds(100), Speed);

        walk.Finish();

        Assert.Equal((new Point2D(3, 1), Facing.Right), (walk.Position, walk.Facing));
        Assert.True(walk.IsFinished);
    }

    // No step kept: the NPC stays where it stands, facing as it did.
    [Fact]
    public void Start_EveryStepOffTheMap_FinishesWhereItStands()
    {
        var walk = new NpcWalk(new Point2D(1, 0), Facing.Left, [Facing.Up, Facing.Up], _layout);

        Assert.True(walk.IsFinished);
        Assert.Equal((new Point2D(1, 0), Facing.Left), (walk.Position, walk.Facing));
    }
}
