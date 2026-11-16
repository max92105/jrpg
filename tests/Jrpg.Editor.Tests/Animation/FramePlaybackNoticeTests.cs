using System.ComponentModel;
using Jrpg.Editor.Animation;
using static Jrpg.Editor.Tests.Animation.FramePlaybackTests;

namespace Jrpg.Editor.Tests.Animation;

// What the shared preview raises for its control (decision 986): each notice when its value changes, none when it
// doesn't, on the frames FramePlaybackTests plays.
public sealed class FramePlaybackNoticeTests
{
    [Fact]
    public void Show_FrameCountChanged_RaisesEachCommandsCanExecute()
    {
        var playback = Playing(frames: 0, milliseconds: 100);
        var raised = (Toggle: 0, Forward: 0, Back: 0);
        playback.TogglePlayingCommand.CanExecuteChanged += (_, _) => raised.Toggle++;
        playback.StepForwardCommand.CanExecuteChanged += (_, _) => raised.Forward++;
        playback.StepBackCommand.CanExecuteChanged += (_, _) => raised.Back++;

        playback.Show(4);

        Assert.Equal((1, 1, 1), raised);
    }

    [Fact]
    public void Advance_IntoTheNextFrame_RaisesTheShownFrameAndItsLine()
    {
        var playback = Playing(frames: 8, milliseconds: 100);

        var raised = Raised(playback, () => playback.Advance(Milliseconds(150)));

        Assert.Equal([nameof(FramePlayback.ShownFrame), nameof(FramePlayback.FrameLine)], raised);
    }

    [Fact]
    public void Advance_WithinTheShownFrame_RaisesNothing()
    {
        var playback = Playing(frames: 8, milliseconds: 100);

        Assert.Empty(Raised(playback, () => playback.Advance(Milliseconds(50))));
    }

    [Fact]
    public void TogglePlaying_Playing_RaisesIsPlaying()
    {
        var playback = Playing(frames: 8, milliseconds: 100);

        var raised = Raised(playback, () => playback.TogglePlayingCommand.Execute(null));

        Assert.Equal([nameof(FramePlayback.IsPlaying)], raised);
    }

    [Fact]
    public void Show_SameCountWhilePlayingTheFirstFrame_RaisesNothing()
    {
        var playback = Playing(frames: 8, milliseconds: 100);

        Assert.Empty(Raised(playback, () => playback.Show(8)));
    }

    [Fact]
    public void Show_AnotherCount_RaisesTheFrameLine()
    {
        var playback = Playing(frames: 8, milliseconds: 100);

        var raised = Raised(playback, () => playback.Show(4));

        Assert.Contains(nameof(FramePlayback.FrameLine), raised);
    }

    [Fact]
    public void HitFrame_Changed_RaisesTheFrameLine()
    {
        var playback = Playing(frames: 8, milliseconds: 100);

        var raised = Raised(playback, () => playback.HitFrame = 0);

        Assert.Equal([nameof(FramePlayback.HitFrame), nameof(FramePlayback.FrameLine)], raised);
    }

    [Fact]
    public void HitFrame_Unchanged_RaisesNothing()
    {
        var playback = Playing(frames: 8, milliseconds: 100);
        playback.HitFrame = 2;

        Assert.Empty(Raised(playback, () => playback.HitFrame = 2));
    }

    private static List<string?> Raised(FramePlayback playback, Action change)
    {
        var raised = new List<string?>();
        ((INotifyPropertyChanged)playback).PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        change();
        return raised;
    }
}
