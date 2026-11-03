namespace Jrpg.Core.Maps;

/// <summary>
/// A tile's animation as content (ADR 0039): how many frames run rightward from the tile's own cell on its sheet's row,
/// and the time each frame shows.
/// </summary>
public sealed record TileAnimation
{
    /// <summary>An animation of one frame is a still tile.</summary>
    public const int MinFrames = 2;

    /// <summary>At least <see cref="MinFrames"/>; the first is the tile's own cell.</summary>
    public required int FrameCount { get; init; }

    /// <summary>How long each frame shows, in whole milliseconds from 1.</summary>
    public required int FrameMilliseconds { get; init; }

    public TimeSpan FrameDuration => TimeSpan.FromMilliseconds(FrameMilliseconds);

    /// <summary>
    /// The frame shown after <paramref name="elapsed"/>, from 0. The shells pass their own time in (the game's update
    /// deltas, the editor's render tick), since Core never reads a clock; every copy of a tile shares one time, so all
    /// are in step. Ticks are counted in <see cref="long"/>, so years of play stay in range.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A negative time: both shells only add deltas.</exception>
    public int FrameAt(TimeSpan elapsed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(elapsed, TimeSpan.Zero);
        return (int)(elapsed.Ticks / FrameDuration.Ticks % FrameCount);
    }
}
