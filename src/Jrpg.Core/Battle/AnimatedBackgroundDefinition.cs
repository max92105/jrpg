using Jrpg.Core.Maps;

namespace Jrpg.Core.Battle;

/// <summary>
/// One animated battle background's frame size and pace, by its file name in <c>backgrounds/</c> (ADR 0059, decision
/// 897): a grid sheet of frames left to right then top to bottom, as many columns as whole frames fit the image's
/// width. A background with no record is a still picture.
/// </summary>
public sealed record AnimatedBackgroundDefinition
{
    /// <summary>The sheet's file name in the backgrounds folder, exact case.</summary>
    public required string File { get; init; }

    /// <summary>One frame's width in the sheet, in pixels from 1.</summary>
    public required int FrameWidth { get; init; }

    /// <summary>One frame's height in the sheet, in pixels from 1.</summary>
    public required int FrameHeight { get; init; }

    /// <summary>How many frames the sheet holds and how long each shows (ADR 0039's shape).</summary>
    public required TileAnimation Animation { get; init; }

    /// <summary>The frame shown after <paramref name="elapsed"/> of the loop, from 0.</summary>
    /// <exception cref="ArgumentOutOfRangeException">A negative time.</exception>
    public int FrameAt(TimeSpan elapsed) => Animation.FrameAt(elapsed);

    /// <summary>
    /// The top-left of <paramref name="frame"/>'s cell in its sheet: the one rule for where a frame sits, which the
    /// game, the editor's preview and the art check all read. That the rows fit the image's height is the art check's.
    /// </summary>
    /// <param name="frame">The frame, from 0.</param>
    /// <param name="imageWidth">The sheet's width in pixels; a part frame at its right edge is not a column.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// A frame outside <c>0..FrameCount - 1</c>, or an image narrower than one frame.
    /// </exception>
    /// <exception cref="OverflowException">
    /// The frame's row lies past what an int can hold, which no record the art check passes reaches; thrown so a
    /// wrapped row never comes out negative.
    /// </exception>
    public (int X, int Y) FrameOrigin(int frame, int imageWidth)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(frame);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(frame, Animation.FrameCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(imageWidth, FrameWidth);
        var columns = imageWidth / FrameWidth;
        return (frame % columns * FrameWidth, checked(frame / columns * FrameHeight));
    }
}
