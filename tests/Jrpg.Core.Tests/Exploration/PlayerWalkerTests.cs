using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Exploration;

public sealed class PlayerWalkerTests
{
    private const double Precision = 9;

    private static readonly ExplorationRules _rules = new() { WalkSpeed = 3, PlayerRadius = 0.1 };
    private static readonly TimeSpan _halfSecond = TimeSpan.FromSeconds(0.5);

    private static readonly MapCollision _openField = new(new ActiveMapBuilder().WithRows("11111", "11111", "11111", "11111", "11111").Build());

    [Fact]
    public void Walk_FullTiltForHalfSecond_MovesSpeedTimesDelta()
    {
        var walker = new PlayerWalker(new Point2D(0.5, 2.5), Facing.Down, _rules);

        walker.Walk(MoveIntent.FromAxes(1, 0), _halfSecond, _openField);

        Assert.Equal(2.0, walker.Position.X, Precision);
        Assert.Equal(2.5, walker.Position.Y, Precision);
    }

    [Fact]
    public void Walk_HalfTilt_MovesHalfAsFar()
    {
        var walker = new PlayerWalker(new Point2D(0.5, 2.5), Facing.Down, _rules);

        walker.Walk(MoveIntent.FromAxes(0.5, 0), _halfSecond, _openField);

        Assert.Equal(1.25, walker.Position.X, Precision);
    }

    [Fact]
    public void Walk_KeyboardDiagonal_CoversTheSameDistanceAsStraight()
    {
        var walker = new PlayerWalker(new Point2D(1, 1), Facing.Down, _rules);

        walker.Walk(MoveIntent.FromAxes(1, 1), _halfSecond, _openField);

        var travelled = Math.Sqrt(Math.Pow(walker.Position.X - 1, 2) + Math.Pow(walker.Position.Y - 1, 2));
        Assert.Equal(1.5, travelled, Precision);
    }

    [Fact]
    public void Walk_DiagonalIntoWall_SlidesAlongIt()
    {
        // A wall fills the bottom row; pushing down-right moves right along it.
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("11111", "22222").Build());
        var walker = new PlayerWalker(new Point2D(1.5, 0.85), Facing.Down, _rules);

        walker.Walk(MoveIntent.FromAxes(1, 1), TimeSpan.FromSeconds(0.1), collision);

        Assert.Equal(1.5 + (0.3 / Math.Sqrt(2)), walker.Position.X, Precision);
        Assert.Equal(0.85, walker.Position.Y, Precision);
    }

    [Fact]
    public void Walk_DiagonalIntoSideWall_SlidesVertically()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("12", "12", "12").Build());
        var walker = new PlayerWalker(new Point2D(0.85, 0.5), Facing.Down, _rules);

        walker.Walk(MoveIntent.FromAxes(1, 1), TimeSpan.FromSeconds(0.1), collision);

        Assert.Equal(0.85, walker.Position.X, Precision);
        Assert.Equal(0.5 + (0.3 / Math.Sqrt(2)), walker.Position.Y, Precision);
    }

    [Fact]
    public void Walk_IntoBlockedCell_KeepsPositionButIsMoving()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("12").Build());
        var walker = new PlayerWalker(new Point2D(0.85, 0.5), Facing.Down, _rules);

        walker.Walk(MoveIntent.FromAxes(1, 0), _halfSecond, collision);

        Assert.Equal(new Point2D(0.85, 0.5), walker.Position);
        Assert.True(walker.IsMoving);
        Assert.Equal(Facing.Right, walker.Facing);
    }

    [Fact]
    public void Walk_PastMapEdge_ClampsToRadius()
    {
        var walker = new PlayerWalker(new Point2D(0.5, 0.5), Facing.Down, _rules);

        walker.Walk(MoveIntent.FromAxes(-1, 0), _halfSecond, _openField);

        Assert.Equal(_rules.PlayerRadius, walker.Position.X, Precision);
    }

    [Fact]
    public void Walk_IntoRightAndBottomEdges_StopsFlushAtRadius()
    {
        var walker = new PlayerWalker(new Point2D(4.5, 4.5), Facing.Down, _rules);

        walker.Walk(MoveIntent.FromAxes(1, 1), _halfSecond, _openField);

        Assert.Equal(new Point2D(5 - _rules.PlayerRadius, 5 - _rules.PlayerRadius), walker.Position);
    }

    [Fact]
    public void Walk_LongFrameTowardThinWall_StopsBeforeIt()
    {
        var collision = new MapCollision(new ActiveMapBuilder().WithRows("11211").Build());
        var walker = new PlayerWalker(new Point2D(1.5, 0.5), Facing.Down, _rules);

        // Three tiles in one frame would land past the wall if only the end point were tested.
        walker.Walk(MoveIntent.FromAxes(1, 0), TimeSpan.FromSeconds(1), collision);

        Assert.InRange(walker.Position.X, 1.5, 2 - _rules.PlayerRadius);
    }

    [Theory]
    [InlineData(0.7, 0.7, Facing.Down)]
    [InlineData(-0.7, -0.7, Facing.Up)]
    [InlineData(0.8, -0.5, Facing.Right)]
    [InlineData(-0.8, 0.5, Facing.Left)]
    public void Walk_Intent_FacesTheLargerAxisAndVerticalOnTie(double x, double y, Facing expected)
    {
        var walker = new PlayerWalker(new Point2D(2.5, 2.5), Facing.Left, _rules);

        walker.Walk(MoveIntent.FromAxes(x, y), TimeSpan.FromSeconds(0.01), _openField);

        Assert.Equal(expected, walker.Facing);
    }

    [Fact]
    public void Walk_NoIntent_StopsMovingAndKeepsFacing()
    {
        var walker = new PlayerWalker(new Point2D(2.5, 2.5), Facing.Down, _rules);
        walker.Walk(MoveIntent.FromAxes(-1, 0), _halfSecond, _openField);
        var stoppedAt = walker.Position;

        walker.Walk(MoveIntent.None, _halfSecond, _openField);

        Assert.False(walker.IsMoving);
        Assert.Equal(Facing.Left, walker.Facing);
        Assert.Equal(stoppedAt, walker.Position);
    }

    [Fact]
    public void PlaceAt_WhileMoving_MovesThereAndStops()
    {
        var walker = new PlayerWalker(new Point2D(2.5, 2.5), Facing.Down, _rules);
        walker.Walk(MoveIntent.FromAxes(1, 0), _halfSecond, _openField);

        walker.PlaceAt(new Point2D(0.5, 0.5));

        Assert.Equal(new Point2D(0.5, 0.5), walker.Position);
        Assert.Equal(new GridCell(0, 0), walker.Cell);
        Assert.False(walker.IsMoving);
    }
}
