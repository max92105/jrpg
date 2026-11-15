using Jrpg.Core.Cinematics;

namespace Jrpg.Core.Tests.Cinematics;

// A still's slow zoom over its time (decision 617): linear from the start percent to the end percent, as a factor.
public sealed class StillZoomTests
{
    private static readonly StillZoom _zoomIn = new() { StartPercent = 100, EndPercent = 108 };
    private static readonly StillZoom _zoomOut = new() { StartPercent = 108, EndPercent = 100 };

    [Fact]
    public void ScaleAt_Start_IsStartPercent()
    {
        Assert.Equal(1.0, _zoomIn.ScaleAt(0), precision: 6);
    }

    [Fact]
    public void ScaleAt_End_IsEndPercent()
    {
        Assert.Equal(1.08, _zoomIn.ScaleAt(1), precision: 6);
    }

    [Fact]
    public void ScaleAt_Half_IsMidway()
    {
        Assert.Equal(1.04, _zoomIn.ScaleAt(0.5), precision: 6);
    }

    [Fact]
    public void ScaleAt_ZoomOut_Shrinks()
    {
        Assert.Equal(1.04, _zoomOut.ScaleAt(0.5), precision: 6);
        Assert.Equal(1.0, _zoomOut.ScaleAt(1), precision: 6);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public void ScaleAt_ShareOutside_Throws(double share)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _zoomIn.ScaleAt(share));
    }
}
