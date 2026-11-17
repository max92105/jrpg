using Jrpg.Data.Art;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// Whole frames written left to right then top to bottom into one sheet no side of which passes
/// <see cref="TextureLimit.MaxSide"/>: an animated battle background (ADR 0059, decision 896), in as many columns as
/// fit the limit's width, so <c>AnimatedBackgroundDefinition.FrameOrigin</c> finds each frame where it was written. WPF
/// imaging needs an STA thread.
/// </summary>
internal static class FrameGrid
{
    /// <returns>As many columns as frames, or as whole frames fit the limit's width when fewer.</returns>
    public static int ColumnsFor(int frameWidth, int frameCount) => Math.Min(frameCount, TextureLimit.MaxSide / frameWidth);

    /// <returns>How many frames a sheet at the limit on both sides holds; 0 when one frame passes it.</returns>
    public static int MostFrames(int frameWidth, int frameHeight) =>
        TextureLimit.MaxSide / frameWidth * (TextureLimit.MaxSide / frameHeight);

    /// <returns>
    /// The sheet's size: its columns by the rows the frames fill, the last row's empty cells counted.
    /// </returns>
    /// <exception cref="ArgumentException">More frames than <see cref="MostFrames"/>.</exception>
    public static (int Width, int Height) SheetSize(int frameWidth, int frameHeight, int frameCount)
    {
        var mostFrames = MostFrames(frameWidth, frameHeight);
        if (frameCount > mostFrames)
        {
            throw new ArgumentException(
                $"{frameCount} frames of {frameWidth} x {frameHeight} px don't fit a {TextureLimit.MaxSide} px sheet "
                    + $"({mostFrames} at most).",
                nameof(frameCount));
        }

        var columns = ColumnsFor(frameWidth, frameCount);
        var rows = (frameCount + columns - 1) / columns;
        return (columns * frameWidth, rows * frameHeight);
    }

    /// <param name="frames">Whole frames of one size, BGRA packed as <see cref="GifFrame.Pixels"/>.</param>
    /// <param name="frameWidth">Each frame's width.</param>
    /// <param name="frameHeight">Each frame's height.</param>
    /// <param name="path">Where the PNG goes; replaced when there. The last row's empty cells are transparent.</param>
    /// <exception cref="ArgumentException">More frames than <see cref="MostFrames"/>; nothing is written.</exception>
    public static void WritePng(IReadOnlyList<uint[]> frames, int frameWidth, int frameHeight, string path)
    {
        var (sheetWidth, sheetHeight) = SheetSize(frameWidth, frameHeight, frames.Count);
        var columns = sheetWidth / frameWidth;
        var sheet = new uint[sheetWidth * sheetHeight];
        for (var index = 0; index < frames.Count; index++)
        {
            var (left, top) = (index % columns * frameWidth, index / columns * frameHeight);
            for (var y = 0; y < frameHeight; y++)
            {
                Array.Copy(frames[index], y * frameWidth, sheet, ((top + y) * sheetWidth) + left, frameWidth);
            }
        }

        PngWriter.Write(sheet, sheetWidth, sheetHeight, path);
    }
}
