using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// One GIF into a strip (ADR 0047), on a two-frame 2 x 2 fixture whose top-left pixel is red, kept 2 px high.
[Collection(GifFixtures.StripFoldersCollection)]
public sealed class GifConversionTests : IDisposable
{
    private const uint Red = 0xFFFF0000;

    private readonly string _folder = Directory.CreateTempSubdirectory("jrpg-gif-").FullName;
    private readonly List<string> _strips = [];

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
        _strips.ForEach(strip => Directory.Delete(Path.GetDirectoryName(strip)!, recursive: true));
    }

    [Fact]
    public void Convert_RemovesBackgroundAsked_TopLeftColorIsTransparent()
    {
        StaThread.Run(() => Assert.Equal(0u, PngPixels.Of(Converted(new GifImportOptions(2, RemovesBackground: true)))[0] >> 24));
    }

    [Fact]
    public void Convert_BackgroundKept_TopLeftStaysRed()
    {
        StaThread.Run(() => Assert.Equal(Red, PngPixels.Of(Converted(new GifImportOptions(2, RemovesBackground: false)))[0]));
    }

    // The fixture's frames last 100 ms, so a typed pace is told from the GIF's own.
    [Fact]
    public void Convert_TypedPace_LoopTakesIt()
    {
        StaThread.Run(() =>
        {
            var converted = Conversion(new GifImportOptions(2, RemovesBackground: false) { FrameMilliseconds = 110 });

            Assert.Equal(110, converted.Loop!.FrameMilliseconds);
        });
    }

    [Fact]
    public void Convert_TypedPace_NoteSaysTyped()
    {
        StaThread.Run(() =>
        {
            var converted = Conversion(new GifImportOptions(2, RemovesBackground: false) { FrameMilliseconds = 110 });

            Assert.Equal("2 frames of 2 x 2 px, the typed 110 ms", converted.Note);
        });
    }

    private string Converted(GifImportOptions options) => Conversion(options).StripPath;

    private ConvertedGif Conversion(GifImportOptions options)
    {
        var frame = new FixtureFrame(0, 0, 2, 2, [1, 2, 2, 1]);
        var gif = GifFixtures.Write(Path.Combine(_folder, "Gnoll.gif"), 2, 2, frame, frame);
        var converted = GifConversion.Convert(gif, options);
        _strips.Add(converted.StripPath);
        return converted;
    }
}
