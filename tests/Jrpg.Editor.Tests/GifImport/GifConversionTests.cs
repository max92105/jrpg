using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// One GIF into a strip (ADR 0047) or a battle background's sheet (ADR 0059), on a two-frame 2 x 2 fixture whose
// top-left pixel is red, kept 2 px high; a background's frames are also built as read, for colors the fixture lacks.
[Collection(GifFixtures.StripFoldersCollection)]
public sealed class GifConversionTests : IDisposable
{
    private const uint Red = 0xFFFF0000;
    private const uint Black = 0xFF000000;
    private const uint White = 0xFFFFFFFF;
    private const uint Grey = 0xFF808080;

    private readonly string _folder = Directory.CreateTempSubdirectory("jrpg-gif-").FullName;
    private readonly List<string> _strips = [];

    private string GifPath => Path.Combine(_folder, "Gnoll.gif");

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

    [Fact]
    public void ConvertToEffect_TypedPace_LoopTakesIt()
    {
        StaThread.Run(() =>
        {
            var converted = Kept(GifConversion.ConvertToEffect(
                Fixture(), new GifImportOptions(2, RemovesBackground: false) { FrameMilliseconds = 110 }));

            Assert.Equal(new TileAnimation { FrameCount = 2, FrameMilliseconds = 110 }, converted.Loop);
        });
    }

    [Fact]
    public void ConvertToBackground_AtTheGifsHeight_WritesTheGridAndItsRecord()
    {
        StaThread.Run(() =>
        {
            var background = BackgroundOf(ComposedGif.Read(Fixture()), new GifImportOptions(2, RemovesBackground: false));

            var animation = new TileAnimation { FrameCount = 2, FrameMilliseconds = 100 };
            var expected = new AnimatedBackgroundDefinition
            {
                File = "Gnoll.png",
                FrameWidth = 2,
                FrameHeight = 2,
                Animation = animation,
            };
            Assert.Equal(expected, background.Background);
            Assert.Equal((4, 2), PngSize.Read(background.StripPath));
        });
    }

    // A 4 x 4 black and white checker halved: each pixel covers two black and two white, which nearest neighbor would
    // give as one of them.
    [Fact]
    public void ConvertToBackground_HalfTheHeight_AveragesTwoByTwo()
    {
        uint[] checker = [.. Enumerable.Range(0, 16).Select(index => ((index / 4) + index) % 2 == 0 ? Black : White)];
        var gif = new ComposedGif([checker, checker], 4, 4, 100);

        StaThread.Run(() =>
        {
            var background = BackgroundOf(gif, new GifImportOptions(2, RemovesBackground: false));

            Assert.Equal([Grey, Grey, Grey, Grey, Grey, Grey, Grey, Grey], PngPixels.Of(background.StripPath));
        });
    }

    [Fact]
    public void ConvertToBackground_TypedFrameTime_IsTheRecords()
    {
        var options = new GifImportOptions(2, RemovesBackground: false) { FrameMilliseconds = 150 };

        StaThread.Run(() => Assert.Equal(150, BackgroundOf(TwoRedFrames(), options).Background!.Animation.FrameMilliseconds));
    }

    [Theory]
    [InlineData(null, "2 frames of 2 x 2 px, 100 ms")]
    [InlineData(150, "2 frames of 2 x 2 px, the typed 150 ms")]
    public void ConvertToBackground_Pace_IsTheNotes(int? typed, string expected)
    {
        var options = new GifImportOptions(2, RemovesBackground: false) { FrameMilliseconds = typed };

        StaThread.Run(() => Assert.Equal(expected, BackgroundOf(TwoRedFrames(), options).Note));
    }

    // The background box ticked, as no dialog leaves it for a background: a background is opaque (decision 904).
    [Fact]
    public void ConvertToBackground_TopLeftColorKept_NothingCleared()
    {
        StaThread.Run(() =>
        {
            var background = BackgroundOf(ComposedGif.Read(Fixture()), new GifImportOptions(2, RemovesBackground: true));

            Assert.Equal(Red, PngPixels.Of(background.StripPath)[0]);
        });
    }

    [Fact]
    public void ConvertToBackground_OneFrameGif_RefusedNamingIt()
    {
        var gif = new ComposedGif([[Red, Red, Red, Red]], 2, 2, 100);

        var refusal = Assert.Throws<InvalidDataException>(
            () => BackgroundOf(gif, new GifImportOptions(2, RemovesBackground: false)));

        Assert.Equal("Gnoll.gif has one frame: import a PNG.", refusal.Message);
    }

    [Fact]
    public void ConvertToBackground_AboveTheGifsHeight_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BackgroundOf(TwoRedFrames(), new GifImportOptions(3, RemovesBackground: false)));
    }

    private static ComposedGif TwoRedFrames() => new([[Red, Red, Red, Red], [Red, Red, Red, Red]], 2, 2, 100);

    private ConvertedGif BackgroundOf(ComposedGif gif, GifImportOptions options) =>
        Kept(GifConversion.ConvertToBackground(GifPath, gif, options));

    private string Converted(GifImportOptions options) => Conversion(options).StripPath;

    private ConvertedGif Conversion(GifImportOptions options) => Kept(GifConversion.Convert(Fixture(), options));

    private string Fixture()
    {
        var frame = new FixtureFrame(0, 0, 2, 2, [1, 2, 2, 1]);
        return GifFixtures.Write(GifPath, 2, 2, frame, frame);
    }

    private ConvertedGif Kept(ConvertedGif converted)
    {
        _strips.Add(converted.StripPath);
        return converted;
    }
}
