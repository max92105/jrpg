using Jrpg.Editor.Animation;

namespace Jrpg.Editor.Tests.Animation;

// The shared preview's rules (decision 986), on frames whose times the tests pick so each frame's start is a round
// number of milliseconds.
public sealed class FramePlaybackTests
{
    [Fact]
    public void Advance_PlayingAtHundredMs_ShowsTheFrameOfTheElapsedTime()
    {
        var playback = Playing(frames: 8, milliseconds: 100);

        playback.Advance(Milliseconds(250));

        Assert.Equal(2, playback.ShownFrame);
    }

    [Fact]
    public void Advance_ToTheLastFramesEnd_ShowsTheLastFrame()
    {
        var playback = Playing(frames: 4, milliseconds: 100);

        playback.Advance(Milliseconds(399));

        Assert.Equal(3, playback.ShownFrame);
    }

    [Fact]
    public void Advance_PastTheLastFrame_Wraps()
    {
        var playback = Playing(frames: 4, milliseconds: 100);
        playback.Advance(Milliseconds(399));

        playback.Advance(Milliseconds(121));

        Assert.Equal(1, playback.ShownFrame);
    }

    [Fact]
    public void Advance_NegativeDelta_Throws()
    {
        var playback = Playing(frames: 4, milliseconds: 100);

        Assert.Throws<ArgumentOutOfRangeException>(() => playback.Advance(TimeSpan.FromTicks(-1)));
    }

    [Fact]
    public void Advance_ZeroDelta_HoldsTheFrame()
    {
        var playback = Playing(frames: 4, milliseconds: 100);
        playback.Advance(Milliseconds(150));

        playback.Advance(TimeSpan.Zero);

        Assert.Equal(1, playback.ShownFrame);
    }

    [Fact]
    public void Advance_Paused_HoldsTheFrame()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.Advance(Milliseconds(150));
        playback.TogglePlayingCommand.Execute(null);

        playback.Advance(Milliseconds(300));

        Assert.Equal((1, false), (playback.ShownFrame, playback.IsPlaying));
    }

    // Paused at 150 ms and played again, 60 ms more reach frame 2; played from the frame's start, it would show 1.
    [Fact]
    public void TogglePlaying_AfterAMidFramePause_PlaysOnFromThePausedMoment()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.Advance(Milliseconds(150));
        playback.TogglePlayingCommand.Execute(null);
        playback.TogglePlayingCommand.Execute(null);

        playback.Advance(Milliseconds(60));

        Assert.Equal(2, playback.ShownFrame);
    }

    [Fact]
    public void Advance_NoFrameTime_HoldsTheFrame()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.Advance(Milliseconds(150));
        playback.FrameMilliseconds = null;

        playback.Advance(Milliseconds(300));

        Assert.Equal(1, playback.ShownFrame);
    }

    [Fact]
    public void Advance_FrameTimeZero_HoldsTheFrame()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.Advance(Milliseconds(150));
        playback.FrameMilliseconds = 0;

        playback.Advance(Milliseconds(300));

        Assert.Equal(1, playback.ShownFrame);
    }

    [Fact]
    public void Advance_FrameTimeOne_Plays()
    {
        var playback = Playing(frames: 8, milliseconds: 1);

        playback.Advance(Milliseconds(3));

        Assert.Equal(3, playback.ShownFrame);
    }

    [Fact]
    public void Advance_NoFrames_ShowsNothing()
    {
        var playback = Playing(frames: 0, milliseconds: 100);

        playback.Advance(Milliseconds(300));

        Assert.Equal((0, ""), (playback.ShownFrame, playback.FrameLine));
    }

    [Fact]
    public void StepForward_AFrame_MovesOneOnAndPauses()
    {
        var playback = Playing(frames: 4, milliseconds: 100);

        playback.StepForwardCommand.Execute(null);

        Assert.Equal((1, false), (playback.ShownFrame, playback.IsPlaying));
    }

    [Fact]
    public void StepForward_LastFrame_WrapsToFirstAndPauses()
    {
        var playback = Playing(frames: 4, milliseconds: 100);
        playback.Advance(Milliseconds(350));

        playback.StepForwardCommand.Execute(null);

        Assert.Equal((0, false), (playback.ShownFrame, playback.IsPlaying));
    }

    [Fact]
    public void StepBack_AFrame_MovesOneBackAndPauses()
    {
        var playback = Playing(frames: 4, milliseconds: 100);
        playback.Advance(Milliseconds(250));

        playback.StepBackCommand.Execute(null);

        Assert.Equal((1, false), (playback.ShownFrame, playback.IsPlaying));
    }

    [Fact]
    public void StepBack_FirstFrame_WrapsToLast()
    {
        var playback = Playing(frames: 4, milliseconds: 100);

        playback.StepBackCommand.Execute(null);

        Assert.Equal((3, false), (playback.ShownFrame, playback.IsPlaying));
    }

    [Fact]
    public void TogglePlaying_AfterAStep_PlaysOnFromTheSteppedFramesStart()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.StepForwardCommand.Execute(null);
        playback.TogglePlayingCommand.Execute(null);

        playback.Advance(Milliseconds(150));

        Assert.Equal(2, playback.ShownFrame);
    }

    [Fact]
    public void Commands_NoFrames_AreGreyed()
    {
        var playback = Playing(frames: 0, milliseconds: 100);

        Assert.Equal((false, false, false), CanExecute(playback));
    }

    [Fact]
    public void Commands_OneFrame_AreLit()
    {
        var playback = Playing(frames: 1, milliseconds: 100);

        Assert.Equal((true, true, true), CanExecute(playback));
    }

    [Fact]
    public void FrameMilliseconds_ChangedWhilePlaying_KeepsTheShownFrame()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.Advance(Milliseconds(350));

        playback.FrameMilliseconds = 300;

        Assert.Equal((3, true), (playback.ShownFrame, playback.IsPlaying));
    }

    [Fact]
    public void FrameMilliseconds_Changed_PlaysOnFromTheShownFramesStart()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.Advance(Milliseconds(350));
        playback.FrameMilliseconds = 300;

        playback.Advance(Milliseconds(300));

        Assert.Equal(4, playback.ShownFrame);
    }

    [Fact]
    public void Show_NewFrames_StartsAtZeroPlaying()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.Advance(Milliseconds(250));
        playback.StepForwardCommand.Execute(null);

        playback.Show(4);

        Assert.Equal((0, true, 4), (playback.ShownFrame, playback.IsPlaying, playback.FrameCount));
    }

    [Fact]
    public void Show_NegativeCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FramePlayback().Show(-1));
    }

    [Fact]
    public void FrameLine_OnTheHitFrame_SaysTheHit()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.HitFrame = 2;

        playback.Advance(Milliseconds(250));

        Assert.Equal("frame 3 of 8, the hit", playback.FrameLine);
    }

    [Fact]
    public void FrameLine_AnotherFrame_IsTheFrameOnly()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.HitFrame = 2;

        playback.Advance(Milliseconds(150));

        Assert.Equal("frame 2 of 8", playback.FrameLine);
    }

    [Fact]
    public void FrameLine_NoFrames_IsEmpty()
    {
        var playback = Playing(frames: 8, milliseconds: 100);

        playback.Show(0);

        Assert.Equal("", playback.FrameLine);
    }

    // Also the notice tests' start: its frame time is set before the frames are shown, as the control hands it.
    internal static FramePlayback Playing(int frames, int milliseconds)
    {
        var playback = new FramePlayback { FrameMilliseconds = milliseconds };
        playback.Show(frames);
        return playback;
    }

    internal static TimeSpan Milliseconds(int milliseconds) => TimeSpan.FromMilliseconds(milliseconds);

    private static (bool Toggle, bool Forward, bool Back) CanExecute(FramePlayback playback) =>
        (playback.TogglePlayingCommand.CanExecute(null), playback.StepForwardCommand.CanExecute(null),
            playback.StepBackCommand.CanExecute(null));
}
