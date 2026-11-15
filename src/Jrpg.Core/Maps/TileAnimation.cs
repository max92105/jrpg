namespace Jrpg.Core.Maps;

/// <summary>
/// A tile's animation as content (ADR 0039): how many frames run rightward from the tile's own cell on its sheet's row,
/// and the time each frame shows. A field sprite's walk is the same shape, from column 0 of a facing's row (ADR 0043),
/// and so is an enemy's idle loop, the frames of its battle strip (ADR 0047).
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
    public int FrameAt(TimeSpan elapsed) => LoopFrameAt(elapsed, FrameDuration, FrameCount);

    /// <summary>
    /// The frame shown after <paramref name="elapsed"/> of a loop of <paramref name="frameCount"/> frames, from 0: the
    /// one loop formula, for <see cref="FrameAt"/> and a field sprite facing's sequence, whose count is its own (ADR
    /// 0054). Ticks are counted in <see cref="long"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A negative time: both shells only add deltas.</exception>
    public static int LoopFrameAt(TimeSpan elapsed, TimeSpan frameTime, int frameCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(elapsed, TimeSpan.Zero);
        return (int)(elapsed.Ticks / frameTime.Ticks % frameCount);
    }
}
