using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

// A zoom over the 1920 x 1080 screen keeps its origin in place (decision 748) and, from 100 % up about a point on the
// screen, never pulls the screen's corners inside it: the picture drawn from the zoomed top-left always covers it.
public sealed class ScreenZoomTests
{
    [Fact]
    public void PositionOf_TheOrigin_StaysPut()
    {
        var zoom = new ScreenZoom(1.5f, new Vector2(480, 810));

        Assert.Equal(new Vector2(480, 810), zoom.PositionOf(new Vector2(480, 810)));
    }

    [Fact]
    public void PositionOf_ScreenCenterAboutTheTopLeft_MovesOutByTheScale()
    {
        var zoom = new ScreenZoom(1.5f, Vector2.Zero);

        Assert.Equal(new Vector2(1440, 810), zoom.PositionOf(new Vector2(960, 540)));
    }

    [Fact]
    public void PositionOf_ScaleOne_LeavesThePoint()
    {
        var zoom = new ScreenZoom(1, new Vector2(300, 900));

        Assert.Equal(new Vector2(700, 200), zoom.PositionOf(new Vector2(700, 200)));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1920, 1080)]
    [InlineData(300, 900)]
    public void PositionOf_ScreenCornersAboutAnOriginOnTheScreen_CoverTheScreen(float originX, float originY)
    {
        var zoom = new ScreenZoom(1.5f, new Vector2(originX, originY));

        var topLeft = zoom.PositionOf(Vector2.Zero);
        var bottomRight = zoom.PositionOf(new Vector2(1920, 1080));

        Assert.True(topLeft.X <= 0 && topLeft.Y <= 0, $"top-left at {topLeft}");
        Assert.True(bottomRight.X >= 1920 && bottomRight.Y >= 1080, $"bottom-right at {bottomRight}");
    }
}
