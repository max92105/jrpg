using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// A GIF read through WPF's decoder (ADR 0047), on fixtures written byte by byte (GifFixtures); WPF imaging runs on an
// STA thread, so each test's body goes through StaThread.
public sealed class GifReaderTests : IDisposable
{
    private const uint Red = 0xFFFF0000;

    private readonly string _folder = Directory.CreateTempSubdirectory("jrpg-gif-").FullName;

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    // Frame 1 is the whole 4 x 4 canvas, shown 10 cs and kept; frame 2 a 2 x 2 patch at (1, 1), 20 cs and cleared;
    // frame 3 the whole canvas again, 30 cs and restored.
    [Fact]
    public void Read_ThreeFrameFixture_ReadsSizeFramesOffsetsAndDelays()
    {
        var path = Fixture(
            4,
            4,
            FixtureFrame.Filled(0, 0, 4, 4, 1) with { DelayCentiseconds = 10, Disposal = 1 },
            FixtureFrame.Filled(1, 1, 2, 2, 2) with { DelayCentiseconds = 20, Disposal = 2 },
            FixtureFrame.Filled(0, 0, 4, 4, 3) with { DelayCentiseconds = 30, Disposal = 3 });

        StaThread.Run(() =>
        {
            var image = GifReader.Read(path);

            Assert.Equal((4, 4), (image.Width, image.Height));
            Assert.Equal(
                [
                    (0, 0, 4, 4, 10, GifDisposal.Keep),
                    (1, 1, 2, 2, 20, GifDisposal.ClearToBackground),
                    (0, 0, 4, 4, 30, GifDisposal.RestorePrevious),
                ],
                image.Frames.Select(frame =>
                    (frame.Left, frame.Top, frame.Width, frame.Height, frame.DelayCentiseconds, frame.Disposal)));
        });
    }

    [Fact]
    public void Read_FrameWithoutTimingBlock_HasNoDelayAndKeeps()
    {
        var path = Fixture(2, 2, FixtureFrame.Filled(0, 0, 2, 2, 1) with { DelayCentiseconds = null });

        StaThread.Run(() =>
        {
            var frame = Assert.Single(GifReader.Read(path).Frames);

            Assert.Equal((0, GifDisposal.Keep), (frame.DelayCentiseconds, frame.Disposal));
        });
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    public void Read_ReservedDisposal_ReadsAsKeep(int disposal)
    {
        var path = Fixture(2, 2, FixtureFrame.Filled(0, 0, 2, 2, 1) with { Disposal = disposal });

        StaThread.Run(() => Assert.Equal(GifDisposal.Keep, Assert.Single(GifReader.Read(path).Frames).Disposal));
    }

    [Fact]
    public void Read_PatchPastTheCanvas_ReadsItsOffsetAsStored()
    {
        var path = Fixture(4, 4, FixtureFrame.Filled(0, 0, 4, 4, 1), FixtureFrame.Filled(2, 2, 3, 3, 2));

        StaThread.Run(() =>
        {
            var patch = GifReader.Read(path).Frames[1];

            Assert.Equal((2, 2, 3, 3), (patch.Left, patch.Top, patch.Width, patch.Height));
        });
    }

    // Color 0 is the transparent one: its pixel reads with alpha 0, the red beside it whole.
    [Fact]
    public void Read_TransparentColor_HasNoAlpha()
    {
        var path = Fixture(2, 1, new FixtureFrame(0, 0, 2, 1, [0, 1]) with { TransparentColor = 0 });

        StaThread.Run(() =>
        {
            var pixels = Assert.Single(GifReader.Read(path).Frames).Pixels;

            Assert.Equal((0u, Red), (pixels[0] >> 24, pixels[1]));
        });
    }

    [Fact]
    public void Read_NotAGif_ThrowsNamingTheFile()
    {
        var path = Path.Combine(_folder, "Notes.gif");
        File.WriteAllText(path, "not a picture");

        StaThread.Run(() =>
        {
            var thrown = Assert.Throws<InvalidDataException>(() => GifReader.Read(path));

            Assert.StartsWith("Notes.gif isn't a GIF the editor can read", thrown.Message, StringComparison.Ordinal);
        });
    }

    private string Fixture(int width, int height, params FixtureFrame[] frames) =>
        GifFixtures.Write(Path.Combine(_folder, $"{Guid.NewGuid():N}.gif"), width, height, frames);
}
