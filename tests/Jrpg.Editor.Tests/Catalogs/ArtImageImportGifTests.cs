using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Tests.GifImport;
using Jrpg.Editor.Tests.Tiles;

namespace Jrpg.Editor.Tests.Catalogs;

/// <summary>
/// Import... taking a GIF as a battle sprite, an attack effect or a battle background (ADR 0047, decision 639, ADR
/// 0059): the test world's records over an empty content folder, the GIFs written by <see cref="GifFixtures"/>: a
/// sprite's 4 x 4 frames at the default 128 px, an effect's 4 x 2 frames at 8 px, a background's 4 x 4 frames at their
/// own 4 px.
/// </summary>
[Collection(GifFixtures.StripFoldersCollection)]
public sealed class ArtImageImportGifTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"jrpg-art-import-{Guid.NewGuid():N}");
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly List<string> _questions = [];
    private readonly ArtImageImport _import;

    public ArtImageImportGifTests()
    {
        _import = new ArtImageImport(new ContentWriter(_root), _draft);
        Directory.CreateDirectory(Path.Combine(_root, "source"));
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Import_GifIntoBattlers_WritesAStripAndItsLoop()
    {
        var gif = Gif("Gnoll.gif", Frame(10), Frame(10), Frame(10));
        var stripFoldersBefore = StripFolders();

        StaThread.Run(() =>
        {
            var result = _import.Import(ArtFolders.Battlers, [gif], GifImportOptions.Default, Ask(answer: true));

            Assert.Equal("Gnoll.png", result.LastImported);
            Assert.Equal((3 * 128, 128), PngSize.Read(Art(ArtFolders.Battlers, "Gnoll.png")));
            Assert.Equal(new TileAnimation { FrameCount = 3, FrameMilliseconds = 100 }, result.Animations["Gnoll.png"]);
        });

        Assert.Equal(stripFoldersBefore, StripFolders());
    }

    // Two frames at 4096 px high make a strip 8192 px wide: the GIF is converted, then Data refuses the strip, and its
    // folder is deleted all the same.
    [Fact]
    public void Import_StripTooWide_IsRefusedAndLeavesNoTemporaryFile()
    {
        var gif = Gif("Giant.gif", Frame(10), Frame(10));
        var stripFoldersBefore = StripFolders();

        StaThread.Run(() =>
        {
            var result = _import.Import(ArtFolders.Battlers, [gif], new GifImportOptions(4096, false), Ask(answer: true));

            Assert.StartsWith("Giant.png: a 8192 px wide image", Assert.Single(result.Refusals), StringComparison.Ordinal);
        });

        Assert.Equal(stripFoldersBefore, StripFolders());
        Assert.False(File.Exists(Art(ArtFolders.Battlers, "Giant.png")));
    }

    [Fact]
    public void Import_OneFrameGif_IsRefusedByName()
    {
        var gif = Gif("Gnoll.gif", Frame(10));

        StaThread.Run(() =>
        {
            var result = _import.Import(ArtFolders.Battlers, [gif], GifImportOptions.Default, Ask(answer: true));

            Assert.Equal(["Gnoll.gif has one frame: import a PNG."], result.Refusals);
            Assert.False(File.Exists(Art(ArtFolders.Battlers, "Gnoll.png")));
        });
    }

    [Fact]
    public void Import_GifIntoPortraits_IsRefusedByName()
    {
        var gif = Gif("Gnoll.gif", Frame(10), Frame(10));

        var result = _import.Import(ArtFolders.Portraits, [gif], GifImportOptions.Default, Ask(answer: true));

        Assert.Equal(
            ["Gnoll.gif: only a battle sprite, an attack effect or a battle background is imported from a GIF."],
            result.Refusals);
        Assert.Empty(_questions);
    }

    [Fact]
    public void Import_GifDeclinedReplace_CopiesNothing()
    {
        var old = PngHeaderFile.Write(Art(ArtFolders.Battlers, "Gnoll.png"), 96, 96);
        var oldBytes = File.ReadAllBytes(old);
        var gif = Gif("Gnoll.gif", Frame(10), Frame(10));

        var result = _import.Import(ArtFolders.Battlers, [gif], GifImportOptions.Default, Ask(answer: false));

        Assert.Equal(["Gnoll.png"], _questions);
        Assert.Null(result.LastImported);
        Assert.Equal(oldBytes, File.ReadAllBytes(old));
    }

    [Fact]
    public void Import_BrokenGif_IsRefusedAndLeavesNoTemporaryFile()
    {
        var gif = Path.Combine(_root, "source", "Broken.gif");
        File.WriteAllText(gif, "not a picture");
        var stripFoldersBefore = StripFolders();

        StaThread.Run(() =>
        {
            var result = _import.Import(ArtFolders.Battlers, [gif], GifImportOptions.Default, Ask(answer: true));

            var refusal = Assert.Single(result.Refusals);
            Assert.StartsWith("Broken.gif isn't a GIF the editor can read", refusal, StringComparison.Ordinal);
        });

        Assert.Equal(stripFoldersBefore, StripFolders());
    }

    [Fact]
    public void Import_GifWithMixedDelays_NotesIt()
    {
        var gif = Gif("Gnoll.gif", Frame(10), Frame(20), Frame(10));

        StaThread.Run(() =>
        {
            var result = _import.Import(ArtFolders.Battlers, [gif], GifImportOptions.Default, Ask(answer: true));

            var note = "3 frames of 128 x 128 px, 100 ms; the GIF's frame times differed, 100 ms is the most common";
            Assert.Equal($"Imported Gnoll.png ({note}).", result.Summary);
        });
    }

    // Frames 4 x 2 at 8 px high: 16 x 8, each padded to a 16 px square, so Data's whole-square-frames check passes.
    [Fact]
    public void Import_GifIntoEffects_WritesWholeSquareFrames()
    {
        var gif = WideGif("Burst.gif", 3);

        StaThread.Run(() => _import.Import(ArtFolders.Effects, [gif], new GifImportOptions(8, false), Ask(answer: true)));

        Assert.Equal((3 * 16, 16), PngSize.Read(Art(ArtFolders.Effects, "Burst.png")));
    }

    // Three frames at the effects' 0.08 s; the fixture's 100 ms dropped, and no loop, as an effect has none.
    [Fact]
    public void Import_GifIntoEffects_ReportsItsLength()
    {
        var gif = WideGif("Burst.gif", 3);

        StaThread.Run(() =>
        {
            var result = _import.Import(ArtFolders.Effects, [gif], new GifImportOptions(8, false), Ask(answer: true));

            var note = "3 frames, 0.24 s; effects play at 80 ms a frame unless effect-strips.json lists them, "
                + "the GIF's 100 ms dropped";
            Assert.Equal($"Imported Burst.png ({note}).", result.Summary);
            Assert.Empty(result.Animations);
        });
    }

    // The pace typed on the Effect strips page, told from the fixture's 100 ms and the unlisted 80 ms.
    [Fact]
    public void Import_EffectGifWithAPace_ReportsItsLoop()
    {
        var gif = WideGif("Burst.gif", 3);

        StaThread.Run(() =>
        {
            var options = new GifImportOptions(8, false) { FrameMilliseconds = 110 };
            var result = _import.Import(ArtFolders.Effects, [gif], options, Ask(answer: true));

            Assert.Equal(new TileAnimation { FrameCount = 3, FrameMilliseconds = 110 }, result.Animations["Burst.png"]);
            var note = "3 frames, 0.33 s, paced at the typed 110 ms; the Effect strips page's OK writes its record";
            Assert.Equal($"Imported Burst.png ({note}).", result.Summary);
        });
    }

    // Three 4 x 4 frames at their own height lie in one row of a 12 x 4 sheet; the typed 110 ms is told from the 100.
    [Fact]
    public void Import_BackgroundGif_WritesTheSheetAndReturnsItsRecord()
    {
        var gif = Gif("Fire.gif", Frame(10), Frame(10), Frame(10));
        var options = new GifImportOptions(4, RemovesBackground: false) { FrameMilliseconds = 110 };

        StaThread.Run(() =>
        {
            var result = _import.Import(ArtFolders.Backgrounds, [gif], options, Ask(answer: true));

            Assert.Equal((12, 4), PngSize.Read(Art(ArtFolders.Backgrounds, "Fire.png")));
            var record = result.Backgrounds["Fire.png"];
            Assert.Equal((4, 4), (record.FrameWidth, record.FrameHeight));
            Assert.Equal(new TileAnimation { FrameCount = 3, FrameMilliseconds = 110 }, record.Animation);
            Assert.Equal("Imported Fire.png (3 frames of 4 x 4 px, the typed 110 ms).", result.Summary);
        });
    }

    // The first GIF is the first one chosen, not the first file: a PNG ahead of it is imported as well.
    [Fact]
    public void Import_TwoBackgroundGifs_RefusesTheSecond()
    {
        var still = PngHeaderFile.Write(Path.Combine(_root, "source", "Still.png"), 64, 36);
        string[] paths = [still, Gif("A.gif", Frame(10), Frame(10)), Gif("B.gif", Frame(10), Frame(10))];

        StaThread.Run(() =>
        {
            var result = _import.Import(ArtFolders.Backgrounds, paths, new GifImportOptions(4, false), Ask(answer: true));

            Assert.Equal(["Still.png", "A.png"], result.Imported);
            Assert.Equal(["B.gif: import one background GIF at a time."], result.Refusals);
        });
    }

    private static FixtureFrame Frame(int centiseconds) =>
        FixtureFrame.Filled(0, 0, 4, 4, 1) with { DelayCentiseconds = centiseconds };

    private static string[] StripFolders() =>
        [.. Directory.GetDirectories(Path.GetTempPath(), $"{GifConversion.TemporaryFolderPrefix}*")
            .Order(StringComparer.Ordinal)];

    private string Gif(string fileName, params FixtureFrame[] frames) =>
        GifFixtures.Write(Path.Combine(_root, "source", fileName), 4, 4, frames);

    private string WideGif(string fileName, int frameCount) =>
        GifFixtures.Write(
            Path.Combine(_root, "source", fileName),
            4,
            2,
            [.. Enumerable.Repeat(FixtureFrame.Filled(0, 0, 4, 2, 1), frameCount)]);

    private string Art(string folder, string fileName) => Path.Combine(ArtFolders.RootIn(_root), folder, fileName);

    private Func<string, bool> Ask(bool answer) => fileName =>
    {
        _questions.Add(fileName);
        return answer;
    };
}
