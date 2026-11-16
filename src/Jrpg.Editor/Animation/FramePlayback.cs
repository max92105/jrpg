using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Animation;

/// <summary>
/// Which frame the shared preview shows (decision 986), headless. Playing, it moves its own time by the render clock's
/// deltas and shows Core's loop frame of it, the rule of every editor animation (<see cref="TileAnimation.FrameAt"/>);
/// paused or stepped, it holds its frame. Its host's typed box is the only frame time: a new one keeps the shown frame,
/// and one that isn't usable holds it.
/// </summary>
internal sealed partial class FramePlayback : ObservableObject
{
    // Kept within one loop from the first frame's start, so the elapsed time stays bounded however long it runs.
    private TimeSpan _elapsed;

    /// <summary>0 when there are no frames to show: every button greyed and the frame line empty.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameLine))]
    [NotifyCanExecuteChangedFor(nameof(TogglePlayingCommand), nameof(StepForwardCommand), nameof(StepBackCommand))]
    public partial int FrameCount { get; private set; }

    /// <summary>From 0, below <see cref="FrameCount"/> when there are frames.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameLine))]
    public partial int ShownFrame { get; private set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; private set; }

    /// <summary>The host's typed frame time, in whole milliseconds; null or below 1 holds the shown frame.</summary>
    [ObservableProperty]
    public partial int? FrameMilliseconds { get; set; }

    /// <summary>The host's typed hit frame, from 0; null while its box isn't a whole number.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FrameLine))]
    public partial int? HitFrame { get; set; }

    /// <summary>"frame 3 of 8", and ", the hit" on <see cref="HitFrame"/>; empty with no frames.</summary>
    public string FrameLine
    {
        get
        {
            if (FrameCount == 0)
            {
                return "";
            }

            var line = $"frame {ShownFrame + 1} of {FrameCount}";
            return HitFrame == ShownFrame ? $"{line}, the hit" : line;
        }
    }

    /// <summary>New frames: the first one shown, playing.</summary>
    /// <param name="frameCount">From 0, which shows nothing.</param>
    public void Show(int frameCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(frameCount);
        FrameCount = frameCount;
        ShowFromStart(0);
        IsPlaying = true;
    }

    /// <summary>Moves a playing preview's time by the render clock's <paramref name="delta"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A negative delta: the render clock only moves on.</exception>
    public void Advance(TimeSpan delta)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delta, TimeSpan.Zero);
        if (!IsPlaying || FrameCount == 0 || FrameTime() is not { } frameTime)
        {
            return;
        }

        _elapsed = TimeSpan.FromTicks((_elapsed + delta).Ticks % (frameTime.Ticks * FrameCount));
        ShownFrame = TileAnimation.LoopFrameAt(_elapsed, frameTime, FrameCount);
    }

    // The shown frame stays, and playing goes on from its start at the new pace.
    partial void OnFrameMillisecondsChanged(int? value) => ShowFromStart(ShownFrame);

    [RelayCommand(CanExecute = nameof(HasFrames))]
    private void TogglePlaying() => IsPlaying = !IsPlaying;

    [RelayCommand(CanExecute = nameof(HasFrames))]
    private void StepForward()
    {
        IsPlaying = false;
        ShowFromStart((ShownFrame + 1) % FrameCount);
    }

    [RelayCommand(CanExecute = nameof(HasFrames))]
    private void StepBack()
    {
        IsPlaying = false;
        ShowFromStart((ShownFrame + FrameCount - 1) % FrameCount);
    }

    private bool HasFrames() => FrameCount > 0;

    // Play resumes a stepped frame from its start, as a stepped frame shows whole.
    private void ShowFromStart(int frame)
    {
        ShownFrame = frame;
        _elapsed = FrameTime() is { } frameTime ? TimeSpan.FromTicks(frameTime.Ticks * frame) : TimeSpan.Zero;
    }

    private TimeSpan? FrameTime() =>
        FrameMilliseconds is { } milliseconds && milliseconds > 0 ? TimeSpan.FromMilliseconds(milliseconds) : null;
}
