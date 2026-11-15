using Jrpg.Core.Cinematics;
using Jrpg.Game.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Exploration;

// The frame a still shows at a time into it, from Core's rules (decision 751): the zoom about its focus on the 1920 x
// 1080 screen, the center without one, at Core's scale and opacity. On a 4 s still zooming 100 to 150 %, built here.
public sealed class StillFrameTests
{
    private static readonly CinematicStill _zooming = new()
    {
        ImageFile = "intro/1.png",
        Seconds = 4,
        Zoom = new StillZoom { StartPercent = 100, EndPercent = 150 },
    };

    [Fact]
    public void At_StillWithoutFocus_ZoomsAboutTheScreenCenter()
    {
        var frame = StillFrame.At(_zooming, TimeSpan.FromSeconds(2));

        Assert.Equal(new Vector2(960, 540), frame.Zoom.Origin);
    }

    [Fact]
    public void At_StillWithFocus_ZoomsAboutItsPointOnTheScreen()
    {
        var frame = StillFrame.At(WithFocus(25, 75), TimeSpan.FromSeconds(2));

        Assert.Equal(new Vector2(480, 810), frame.Zoom.Origin);
    }

    [Fact]
    public void At_FocusAtTheBottomRightEdge_IsTheScreensCorner()
    {
        var frame = StillFrame.At(WithFocus(100, 100), TimeSpan.FromSeconds(2));

        Assert.Equal(new Vector2(1920, 1080), frame.Zoom.Origin);
    }

    [Fact]
    public void At_HalfwayThroughItsTime_ScalesByCoresZoom()
    {
        var frame = StillFrame.At(_zooming, TimeSpan.FromSeconds(2));

        Assert.Equal(1.25f, frame.Zoom.Scale, precision: 5);
    }

    [Fact]
    public void At_IntoTheFadeIn_TakesCoresOpacity()
    {
        var frame = StillFrame.At(_zooming, TimeSpan.FromSeconds(0.2));

        Assert.Equal(StillFade.OpacityAt(TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(4)), frame.Opacity);
        Assert.InRange(frame.Opacity, 0.01f, 0.99f);
    }

    private static CinematicStill WithFocus(int xPercent, int yPercent) =>
        _zooming with { Focus = new StillFocus { XPercent = xPercent, YPercent = yPercent } };
}
