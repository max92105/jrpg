using Jrpg.Core.Cinematics;

namespace Jrpg.Core.Tests.Cinematics;

// How large a 4 s still draws at a time into it (decision 617): its zoom over its time, 1 without one.
public sealed class CinematicStillTests
{
    private static readonly CinematicStill _zooming = new()
    {
        ImageFile = "intro/1.png",
        Seconds = 4,
        Zoom = new StillZoom { StartPercent = 100, EndPercent = 110 },
    };

    [Fact]
    public void ScaleAt_NoZoom_IsOne()
    {
        Assert.Equal(1.0, (_zooming with { Zoom = null }).ScaleAt(TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public void ScaleAt_HalfItsTime_IsMidway()
    {
        Assert.Equal(1.05, _zooming.ScaleAt(TimeSpan.FromSeconds(2)), precision: 6);
    }

    [Fact]
    public void ScaleAt_PastItsTime_StaysAtTheEnd()
    {
        Assert.Equal(1.1, _zooming.ScaleAt(TimeSpan.FromSeconds(5)), precision: 6);
    }
}
