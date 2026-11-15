using Jrpg.Core.Cinematics;
using Jrpg.Core.Geometry;

namespace Jrpg.Core.Tests.Cinematics;

// How large a 4 s still draws at a time into it (decision 617): its zoom over its time, 1 without one; and the point
// the zoom keeps in place (decision 751): its focus, else the center.
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

    [Fact]
    public void ZoomOrigin_NoFocus_IsTheCenter()
    {
        Assert.Equal(new Point2D(0.5, 0.5), _zooming.ZoomOrigin);
    }

    [Fact]
    public void ZoomOrigin_WithFocus_IsItsShare()
    {
        var focused = _zooming with { Focus = new StillFocus { XPercent = 20, YPercent = 70 } };

        Assert.Equal(new Point2D(0.2, 0.7), focused.ZoomOrigin);
    }

    // The focus moves the origin, never the scale.
    [Fact]
    public void ScaleAt_WithFocus_IsUnchanged()
    {
        var focused = _zooming with { Focus = new StillFocus { XPercent = 20, YPercent = 70 } };

        Assert.Equal(1.05, focused.ScaleAt(TimeSpan.FromSeconds(2)), precision: 6);
    }
}
