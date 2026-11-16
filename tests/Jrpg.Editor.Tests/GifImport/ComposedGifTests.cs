using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// A GIF read once for the import dialog's preview (decision 987), on fixtures written byte by byte (GifFixtures); WPF
// imaging runs on an STA thread.
public sealed class ComposedGifTests : IDisposable
{
    private const uint Red = 0xFFFF0000;
    private const uint Green = 0xFF00FF00;

    private readonly string _folder = Directory.CreateTempSubdirectory("jrpg-gif-").FullName;

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    // A 3 x 2 red canvas, then a 1 x 1 green patch at (1, 1): the second frame is whole, the patch over the red.
    [Fact]
    public void Read_TwoFrameGif_ComposesBothAtItsSize()
    {
        var path = Fixture(FixtureFrame.Filled(0, 0, 3, 2, 1), FixtureFrame.Filled(1, 1, 1, 1, 2));

        StaThread.Run(() =>
        {
            var gif = ComposedGif.Read(path);

            Assert.Equal((3, 2, 2), (gif.Width, gif.Height, gif.Frames.Count));
            Assert.Equal([Red, Red, Red, Red, Green, Red], gif.Frames[1]);
        });
    }

    [Fact]
    public void Read_DelaysDiffer_TakesTheMostCommon()
    {
        var frame = FixtureFrame.Filled(0, 0, 3, 2, 1);
        var path = Fixture(
            frame with { DelayCentiseconds = 10 },
            frame with { DelayCentiseconds = 30 },
            frame with { DelayCentiseconds = 30 });

        StaThread.Run(() => Assert.Equal(300, ComposedGif.Read(path).FrameMilliseconds));
    }

    // The dialog plays it held; the conversion is what refuses a one-frame GIF.
    [Fact]
    public void Read_OneFrameGif_IsRead()
    {
        var path = Fixture(FixtureFrame.Filled(0, 0, 3, 2, 1));

        StaThread.Run(() => Assert.Equal([Red, Red, Red, Red, Red, Red], Assert.Single(ComposedGif.Read(path).Frames)));
    }

    // A canvas with no width or no height gives frames with no pixel, which the preview can't make into an image.
    [Theory]
    [InlineData(0, 2)]
    [InlineData(2, 0)]
    public void Read_EmptyCanvas_RefusedNamingTheFile(int width, int height)
    {
        var path = GifFixtures.Write(Path.Combine(_folder, "Flat.gif"), width, height, FixtureFrame.Filled(0, 0, 2, 2, 1));

        StaThread.Run(() =>
        {
            var refusal = Assert.Throws<InvalidDataException>(() => ComposedGif.Read(path));

            Assert.Equal($"Flat.gif has an empty canvas ({width} x {height} px).", refusal.Message);
        });
    }

    private string Fixture(params FixtureFrame[] frames) =>
        GifFixtures.Write(Path.Combine(_folder, "Gnoll.gif"), 3, 2, frames);
}
