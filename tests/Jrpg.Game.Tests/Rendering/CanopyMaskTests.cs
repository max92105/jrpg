using Jrpg.Core.Geometry;
using Jrpg.Game.Rendering;

namespace Jrpg.Game.Tests.Rendering;

public sealed class CanopyMaskTests
{
    // The top half of the object, as a 0..1 outline.
    private static readonly IReadOnlyList<Point2D> _topHalf = [new(0, 0), new(1, 0), new(1, 0.5), new(0, 0.5)];

    [Fact]
    public void Build_PixelInsideZone_IsTrue()
    {
        var mask = CanopyMask.Build([_topHalf], width: 4, height: 4);

        Assert.True(mask[(1 * 4) + 2]);
    }

    [Fact]
    public void Build_PixelOutsideEveryZone_IsFalse()
    {
        var mask = CanopyMask.Build([_topHalf], width: 4, height: 4);

        Assert.False(mask[(2 * 4) + 2]);
    }

    [Fact]
    public void Build_TopHalfZone_CoversExactlyTheTopRows()
    {
        var mask = CanopyMask.Build([_topHalf], width: 2, height: 4);

        Assert.Equal([true, true, true, true, false, false, false, false], mask);
    }

    [Fact]
    public void Build_TwoZones_CoversBoth()
    {
        IReadOnlyList<Point2D> bottomLeft = [new(0, 0.5), new(0.5, 0.5), new(0.5, 1), new(0, 1)];

        var mask = CanopyMask.Build([_topHalf, bottomLeft], width: 2, height: 2);

        Assert.Equal([true, true, true, false], mask);
    }
}
