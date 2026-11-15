using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// Whole frames written side by side as one PNG strip, one row (ADR 0047); an attack effect's each padded to a square
/// first (decision 57). WPF imaging needs an STA thread.
/// </summary>
internal static class FrameStrip
{
    private const int BytesPerPixel = 4;
    private const double Dpi = 96;

    /// <summary>
    /// Each frame centered in a square of its larger side, transparent around, then side by side: an effect strip.
    /// </summary>
    /// <param name="frames">Whole frames of one size, BGRA packed as <see cref="GifFrame.Pixels"/>.</param>
    /// <param name="frameWidth">Each frame's width.</param>
    /// <param name="frameHeight">Each frame's height.</param>
    /// <param name="path">Where the PNG goes; replaced when there.</param>
    public static void WriteSquarePng(IReadOnlyList<uint[]> frames, int frameWidth, int frameHeight, string path)
    {
        var side = Math.Max(frameWidth, frameHeight);
        var (left, top) = ((side - frameWidth) / 2, (side - frameHeight) / 2);
        var squares = frames.Select(frame =>
        {
            var square = new uint[side * side];
            for (var y = 0; y < frameHeight; y++)
            {
                Array.Copy(frame, y * frameWidth, square, ((top + y) * side) + left, frameWidth);
            }

            return square;
        });
        WritePng([.. squares], side, side, path);
    }

    /// <param name="frames">Whole frames of one size, BGRA packed as <see cref="GifFrame.Pixels"/>.</param>
    /// <param name="frameWidth">Each frame's width.</param>
    /// <param name="frameHeight">Each frame's height, the strip's.</param>
    /// <param name="path">Where the PNG goes; replaced when there.</param>
    public static void WritePng(IReadOnlyList<uint[]> frames, int frameWidth, int frameHeight, string path)
    {
        var stripWidth = frames.Count * frameWidth;
        var strip = new uint[stripWidth * frameHeight];
        for (var index = 0; index < frames.Count; index++)
        {
            for (var y = 0; y < frameHeight; y++)
            {
                Array.Copy(frames[index], y * frameWidth, strip, (y * stripWidth) + (index * frameWidth), frameWidth);
            }
        }

        var stride = stripWidth * BytesPerPixel;
        var image = BitmapSource.Create(stripWidth, frameHeight, Dpi, Dpi, PixelFormats.Bgra32, null, strip, stride);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var file = File.Create(path);
        encoder.Save(file);
    }
}
