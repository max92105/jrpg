using Jrpg.Core.Geometry;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class GridSnapTests
{
    [Fact]
    public void TopLeft_JustUnderHalf_RoundsDown()
    {
        Assert.Equal(new Point2D(3, 2), GridSnap.TopLeft(new Point2D(3.49, 2.2)));
    }

    [Fact]
    public void TopLeft_Half_RoundsToTheNextCell()
    {
        Assert.Equal(new Point2D(4, 3), GridSnap.TopLeft(new Point2D(3.5, 2.5)));
    }

    [Fact]
    public void TopLeft_NegativeOverMinusHalf_RoundsToZero()
    {
        Assert.Equal(new Point2D(0, 0), GridSnap.TopLeft(new Point2D(-0.4, 0)));
    }

    [Fact]
    public void TopLeft_NegativeUnderMinusHalf_RoundsToMinusOne()
    {
        Assert.Equal(new Point2D(-1, 0), GridSnap.TopLeft(new Point2D(-0.6, 0)));
    }

    [Fact]
    public void TopLeft_WholeCell_StaysPut()
    {
        Assert.Equal(new Point2D(5, 7), GridSnap.TopLeft(new Point2D(5, 7)));
    }
}
