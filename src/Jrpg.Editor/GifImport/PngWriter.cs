using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jrpg.Editor.GifImport;

/// <summary>
/// Pixels written as a PNG file, for a strip (<see cref="FrameStrip"/>) or a background sheet
/// (<see cref="FrameGrid"/>). WPF imaging needs an STA thread.
/// </summary>
internal static class PngWriter
{
    private const int BytesPerPixel = 4;
    private const double Dpi = 96;

    /// <param name="pixels">Row by row, BGRA packed as <see cref="GifFrame.Pixels"/>.</param>
    /// <param name="width">The image's width in pixels.</param>
    /// <param name="height">The image's height in pixels.</param>
    /// <param name="path">Where the PNG goes; replaced when there.</param>
    public static void Write(uint[] pixels, int width, int height, string path)
    {
        var image = BitmapSource.Create(width, height, Dpi, Dpi, PixelFormats.Bgra32, null, pixels, width * BytesPerPixel);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var file = File.Create(path);
        encoder.Save(file);
    }
}
