using Jrpg.Core.Geometry;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ShapeEditingTests
{
    // A 100 px square, in screen pixels: points 0 to 3 clockwise from the top left.
    private static readonly Point2D[] _square = [new(0, 0), new(100, 0), new(100, 100), new(0, 100)];

    [Fact]
    public void HandleAt_MouseWithinRadius_ReturnsNearestPoint()
    {
        Assert.Equal(1, ShapeEditing.HandleAt(_square, new Point2D(97, 3), radius: 6));
    }

    [Fact]
    public void HandleAt_MouseFarFromEveryPoint_ReturnsNull()
    {
        Assert.Null(ShapeEditing.HandleAt(_square, new Point2D(50, 50), radius: 6));
    }

    // The closing edge runs from the last point (0, 100) back to the first (0, 0).
    [Fact]
    public void EdgeAt_MouseNearClosingEdge_ReturnsLastIndex()
    {
        Assert.Equal(3, ShapeEditing.EdgeAt(_square, new Point2D(5, 50), distance: 8));
    }

    [Fact]
    public void EdgeAt_MouseNearTopEdge_ReturnsItsFirstIndex()
    {
        Assert.Equal(0, ShapeEditing.EdgeAt(_square, new Point2D(50, -4), distance: 8));
    }

    [Fact]
    public void EdgeAt_MouseFarFromEveryEdge_ReturnsNull()
    {
        Assert.Null(ShapeEditing.EdgeAt(_square, new Point2D(50, 50), distance: 8));
    }
}
