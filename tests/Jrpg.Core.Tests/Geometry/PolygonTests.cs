using Jrpg.Core.Geometry;

namespace Jrpg.Core.Tests.Geometry;

public sealed class PolygonTests
{
    // A "U" shape: the notch (0.4..0.6, 0..0.6) is outside.
    private static readonly Point2D[] _cup =
    [
        new(0, 0), new(0.4, 0), new(0.4, 0.6), new(0.6, 0.6), new(0.6, 0), new(1, 0), new(1, 1), new(0, 1),
    ];

    [Fact]
    public void Contains_PointInConcaveNotch_ReturnsFalse()
    {
        var isInside = Polygon.Contains(_cup, new Point2D(0.5, 0.3));

        Assert.False(isInside);
    }

    [Fact]
    public void Contains_PointInEitherArm_ReturnsTrue()
    {
        Assert.True(Polygon.Contains(_cup, new Point2D(0.2, 0.3)));
        Assert.True(Polygon.Contains(_cup, new Point2D(0.8, 0.3)));
    }

    // A point exactly on a horizontal edge counts as below it: the notch's floor (y = 0.6) belongs to the solid bottom.
    [Theory]
    [InlineData(0.5, 0.59, false)]
    [InlineData(0.5, 0.6, true)]
    [InlineData(0.2, 0.6, true)]
    public void Contains_PointLevelWithHorizontalEdge_ReturnsExpectedSide(double x, double y, bool expected)
    {
        var isInside = Polygon.Contains(_cup, new Point2D(x, y));

        Assert.Equal(expected, isInside);
    }

    [Fact]
    public void Contains_PointOutsideBounds_ReturnsFalse()
    {
        var isInside = Polygon.Contains(_cup, new Point2D(1.5, 0.5));

        Assert.False(isInside);
    }
}
