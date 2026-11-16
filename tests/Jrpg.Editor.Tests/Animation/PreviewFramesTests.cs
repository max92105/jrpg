using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Animation;

namespace Jrpg.Editor.Tests.Animation;

// A strip cut into the frames a preview plays, each frame of the test's strip one color, built in memory.
public sealed class PreviewFramesTests
{
    private const uint Red = 0xFFFF0000;
    private const uint Green = 0xFF00FF00;
    private const uint Blue = 0xFF0000FF;
    private const int Side = 4;

    [Fact]
    public void OfStrip_ThreeSquareFrames_CutsThree()
    {
        StaThread.Run(() =>
        {
            var frames = PreviewFrames.OfStrip(StripOf([Red, Green, Blue], Side), 3);

            Assert.Equal([Red, Green, Blue], frames.Select(FirstPixel));
            Assert.All(frames, frame => Assert.Equal((Side, Side), (frame.PixelWidth, frame.PixelHeight)));
        });
    }

    [Fact]
    public void OfStrip_WidthNotSplitting_IsEmpty()
    {
        StaThread.Run(() => Assert.Empty(PreviewFrames.OfStrip(StripOf([Red, Green], Side), 3)));
    }

    // An image taller than wide has no whole frame (its strip's count is 0): no frames rather than a division by zero.
    [Fact]
    public void OfStrip_NoFrames_IsEmpty()
    {
        StaThread.Run(() => Assert.Empty(PreviewFrames.OfStrip(StripOf([Red, Green], Side), 0)));
    }

    // Each color a square frame of the given side, left to right.
    private static BitmapSource StripOf(uint[] colors, int side)
    {
        var width = colors.Length * side;
        var pixels = Enumerable.Range(0, width * side).Select(index => colors[index % width / side]).ToArray();
        return BitmapSource.Create(width, side, 96, 96, PixelFormats.Bgra32, null, pixels, width * sizeof(uint));
    }

    private static uint FirstPixel(BitmapSource frame)
    {
        var pixel = new uint[1];
        new CroppedBitmap(frame, new Int32Rect(0, 0, 1, 1)).CopyPixels(pixel, sizeof(uint), 0);
        return pixel[0];
    }
}
