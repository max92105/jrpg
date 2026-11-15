using Jrpg.Data.Art;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// Whole frames side by side as one PNG strip, read back from the file.
public sealed class FrameStripTests : IDisposable
{
    private const uint Red = 0xFFFF0000;
    private const uint Green = 0xFF00FF00;
    private const uint Blue = 0xFF0000FF;

    private readonly string _folder = Directory.CreateTempSubdirectory("jrpg-strip-").FullName;

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Fact]
    public void WritePng_ThreeFrames_IsThreeWideOneHigh()
    {
        var path = Path.Combine(_folder, "Strip.png");

        StaThread.Run(() =>
        {
            FrameStrip.WritePng([[Red, Red], [Green, Blue], [Blue, Blue]], 2, 1, path);

            var pixels = PngPixels.Of(path);
            Assert.Equal((6, 1), PngSize.Read(path));
            Assert.Equal((Green, Blue), (pixels[2], pixels[3]));
        });
    }

    // Two 4 x 2 frames: each centered in a 4 px square, a transparent row above and below it.
    [Fact]
    public void WriteSquarePng_WideFrames_PadsToSquares()
    {
        var path = Path.Combine(_folder, "Wide.png");

        StaThread.Run(() =>
        {
            FrameStrip.WriteSquarePng([[.. Enumerable.Repeat(Red, 8)], [.. Enumerable.Repeat(Green, 8)]], 4, 2, path);

            var pixels = PngPixels.Of(path);
            Assert.Equal((8, 4), PngSize.Read(path));
            Assert.Equal((0u, Red, Red, 0u, Green), (pixels[0] >> 24, pixels[8], pixels[16], pixels[24] >> 24, pixels[12]));
        });
    }

    [Fact]
    public void WriteSquarePng_SquareFrames_LeavesThem()
    {
        var path = Path.Combine(_folder, "Square.png");

        StaThread.Run(() =>
        {
            FrameStrip.WriteSquarePng([[Red, Green, Blue, Red], [Blue, Blue, Green, Green]], 2, 2, path);

            Assert.Equal([Red, Green, Blue, Blue, Blue, Red, Green, Green], PngPixels.Of(path));
        });
    }
}
