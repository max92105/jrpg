using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

// A screen over the map fades in from its first drawn frame and out once asked (decision 370).
public sealed class OverlayFadeTests
{
    [Fact]
    public void Opacity_BeforeFirstDraw_StaysClear()
    {
        var fade = new OverlayFade();

        fade.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(0, fade.Opacity);
    }

    [Fact]
    public void Opacity_AfterTheOpeningTimeFromTheFirstDraw_IsOpaque()
    {
        var fade = new OverlayFade();
        fade.MarkDrawn();

        fade.Advance(TimeSpan.FromSeconds(0.15));

        Assert.Equal(1, fade.Opacity);
    }

    // The opening counts from the first drawn frame, so a slow first frame can't use it up.
    [Fact]
    public void Opacity_AfterASlowFirstFrame_StartsFromTheFirstDraw()
    {
        var fade = new OverlayFade();
        fade.Advance(TimeSpan.FromSeconds(1));
        fade.MarkDrawn();

        fade.Advance(TimeSpan.FromSeconds(0.075));

        Assert.Equal(0.5f, fade.Opacity, 0.01f);
    }

    [Fact]
    public void IsClosingDone_AfterTheClosingTime_IsTrue()
    {
        var fade = new OverlayFade();
        fade.MarkDrawn();
        fade.Advance(TimeSpan.FromSeconds(1));
        fade.StartClosing();

        fade.Advance(TimeSpan.FromSeconds(0.12));

        Assert.True(fade.IsClosing);
        Assert.True(fade.IsClosingDone);
        Assert.Equal(0, fade.Opacity);
    }

    [Fact]
    public void IsClosingDone_BeforeTheClosingTime_IsFalse()
    {
        var fade = new OverlayFade();
        fade.MarkDrawn();
        fade.Advance(TimeSpan.FromSeconds(1));
        fade.StartClosing();

        fade.Advance(TimeSpan.FromSeconds(0.06));

        Assert.False(fade.IsClosingDone);
        Assert.Equal(0.5f, fade.Opacity, 0.01f);
    }
}
