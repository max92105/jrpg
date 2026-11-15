using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Jrpg.Editor.Tests.GifImport;

// A PNG the tests wrote, read back as BGRA packed pixels; WPF imaging, so on an STA thread.
internal static class PngPixels
{
    public static uint[] Of(string path)
    {
        var decoded = BitmapFrame.Create(new Uri(path), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        var bgra = new FormatConvertedBitmap(decoded, PixelFormats.Bgra32, null, 0);
        var pixels = new uint[bgra.PixelWidth * bgra.PixelHeight];
        bgra.CopyPixels(pixels, bgra.PixelWidth * 4, 0);
        return pixels;
    }
}
