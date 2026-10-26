using Jrpg.Game.Menus;

namespace Jrpg.Game.Tests.Menus;

public sealed class MenuMotionTests
{
    [Fact]
    public void TabOffset_FirstTabAtOpening_IsTheFullSlide()
    {
        var motion = new MenuMotion();

        motion.MarkDrawn();

        Assert.Equal(MenuMotion.TabSlideDistance, motion.TabOffset(0));
    }

    [Fact]
    public void TabOffset_SecondTabBeforeItsDelay_IsTheFullSlide()
    {
        var motion = new MenuMotion();
        motion.MarkDrawn();

        motion.Advance(TimeSpan.FromSeconds(0.02));

        Assert.Equal(MenuMotion.TabSlideDistance, motion.TabOffset(1));
        Assert.True(motion.TabOffset(0) < MenuMotion.TabSlideDistance);
    }

    [Fact]
    public void TabOffset_AfterTheEntrance_IsZero()
    {
        var motion = new MenuMotion();
        motion.MarkDrawn();

        motion.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(0, motion.TabOffset(5));
        Assert.Equal(1, motion.FrameOpacity);
    }

    [Fact]
    public void FrameOpacity_HalfwayThroughClosing_IsBetweenZeroAndOne()
    {
        var motion = OpenedMotion();
        motion.StartClosing();

        motion.Advance(TimeSpan.FromSeconds(0.06));

        Assert.InRange(motion.FrameOpacity, 0.01f, 0.99f);
    }

    [Fact]
    public void IsClosingDone_BeforeClosingStarts_IsFalse()
    {
        var motion = OpenedMotion();

        Assert.False(motion.IsClosingDone);
    }

    [Fact]
    public void IsClosingDone_AfterTheClosingFade_IsTrue()
    {
        var motion = OpenedMotion();
        motion.StartClosing();

        motion.Advance(TimeSpan.FromSeconds(0.12));

        Assert.True(motion.IsClosingDone);
        Assert.Equal(0, motion.FrameOpacity);
    }

    [Fact]
    public void StartClosing_SecondCall_KeepsTheFadeGoing()
    {
        var motion = OpenedMotion();
        motion.StartClosing();
        motion.Advance(TimeSpan.FromSeconds(0.1));

        motion.StartClosing();
        motion.Advance(TimeSpan.FromSeconds(0.02));

        Assert.True(motion.IsClosingDone);
    }

    [Fact]
    public void FrameOpacity_AdvanceBeforeFirstDraw_StaysClear()
    {
        var motion = new MenuMotion();

        for (var update = 0; update < 30; update++)
        {
            motion.Advance(TimeSpan.FromSeconds(1.0 / 60));
        }

        Assert.Equal(0, motion.FrameOpacity);
        Assert.Equal(MenuMotion.TabSlideDistance, motion.TabOffset(0));
    }

    [Fact]
    public void TabOpacity_AtOpeningAndAfterClosing_IsZero()
    {
        var opening = new MenuMotion();
        opening.MarkDrawn();
        var closed = OpenedMotion();
        closed.StartClosing();

        closed.Advance(TimeSpan.FromSeconds(0.12));

        Assert.Equal(0, opening.TabOpacity(0));
        Assert.Equal(0, closed.TabOpacity(0));
    }

    private static MenuMotion OpenedMotion()

    {
        var motion = new MenuMotion();
        motion.MarkDrawn();
        motion.Advance(TimeSpan.FromSeconds(1));
        return motion;
    }
}
