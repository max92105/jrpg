using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Battle;

// An import entering the Effect strips session (decision 988): the test world's catalog with records built here, over a
// folder the test lists out of order: Burst.png (four 8 x 8 frames, no record) and Slash.png (four frames, listed at 60
// ms, hit frame 2, Burst.png its swing), and a record for Gone.png, an image the folder lacks. An import sets the new
// size of the file it copies before handing its result over, as the shell's lookup reads the folder again.
public sealed class EffectStripImportTests
{
    private static readonly ContentCatalog _testWorld =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly EffectStripDefinition _slash =
        new() { File = "Slash.png", FrameMilliseconds = 60, HitFrame = 2, SwingFile = "Burst.png" };

    private static readonly EffectStripDefinition _gone = new() { File = "Gone.png", FrameMilliseconds = 40 };

    private readonly Dictionary<string, (int Width, int Height)> _sizes = new(StringComparer.Ordinal)
    {
        ["Burst.png"] = (32, 8),
        ["Slash.png"] = (32, 8),
    };

    [Fact]
    public void TakeImport_NewImage_InsertedInNameOrderAndChosen()
    {
        var catalog = Catalog();

        catalog.TakeImport(Imported("Fire.png", (24, 8)));

        Assert.Equal(["Burst.png", "Fire.png", "Slash.png", "Gone.png"], catalog.Strips.Select(strip => strip.File));
        var fire = catalog.SelectedStrip!;
        Assert.Equal(("Fire.png", 3, false), (fire.File, fire.FrameCount, fire.IsListed));
    }

    [Fact]
    public void TakeImport_NewGifWithAPace_RowTypedAtItsPace()
    {
        var catalog = Catalog();

        catalog.TakeImport(Imported("Fire.png", (24, 8)) with { Animations = PaceOf("Fire.png", 70) });

        Assert.Equal("70", catalog.SelectedStrip!.FrameMillisecondsText);
        Assert.Contains(new EffectStripDefinition { File = "Fire.png", FrameMilliseconds = 70 }, catalog.ToStrips());
    }

    [Fact]
    public void TakeImport_ReplacedImage_KeepsSwingAndHitWithinTheNewFrames()
    {
        var catalog = Catalog();

        catalog.TakeImport(Imported("Slash.png", (24, 8)) with { Animations = PaceOf("Slash.png", 70) });

        var slash = catalog.SelectedStrip!;
        Assert.Equal(("Slash.png", 3), (slash.File, slash.FrameCount));
        Assert.Equal(("70", "2", "Burst.png"), (slash.FrameMillisecondsText, slash.HitFrameText, slash.SwingFile));
    }

    // Two frames now: hit frame 2 is just past them.
    [Fact]
    public void TakeImport_ReplacedImageWithFewerFrames_HitBackToZero()
    {
        var catalog = Catalog();

        catalog.TakeImport(Imported("Slash.png", (16, 8)));

        Assert.Equal(("0", null), (catalog.SelectedStrip!.HitFrameText, catalog.Problem));
    }

    [Fact]
    public void TakeImport_ReplacedImageWithoutAPace_DropsTheTypedBoxesForItsRecord()
    {
        var catalog = Catalog();
        catalog.Choose("Slash.png");
        catalog.SelectedStrip!.FrameMillisecondsText = "45";

        catalog.TakeImport(Imported("Slash.png", (32, 8)));

        Assert.Equal("60", catalog.SelectedStrip.FrameMillisecondsText);
        Assert.Equal([_gone, _slash], catalog.ToStrips());
    }

    // Gone.png's row sat among the missing images, after every image of the folder.
    [Fact]
    public void TakeImport_MissingRecordsImage_JoinsTheFolderImagesWithItsRecord()
    {
        var catalog = Catalog();

        catalog.TakeImport(Imported("Gone.png", (16, 8)));

        Assert.Equal(["Burst.png", "Gone.png", "Slash.png"], catalog.Strips.Select(strip => strip.File));
        Assert.Equal(("40", 2), (catalog.SelectedStrip!.FrameMillisecondsText, catalog.SelectedStrip.FrameCount));
    }

    [Fact]
    public void TakeImport_NewImage_OfferedAsASwing()
    {
        var catalog = Catalog();

        catalog.TakeImport(Imported("Fire.png", (24, 8)));

        Assert.Equal(["Burst.png", "Fire.png", "Slash.png"], catalog.SwingChoices);
    }

    [Fact]
    public void TakeImport_ReplacedImage_OfferedAsASwingOnce()
    {
        var catalog = Catalog();

        catalog.TakeImport(Imported("Slash.png", (24, 8)));

        Assert.Equal(["Burst.png", "Slash.png"], catalog.SwingChoices);
    }

    [Fact]
    public void TakeImport_NothingCopied_KeepsTheRowsAndTheChoice()
    {
        var catalog = Catalog();
        catalog.Choose("Slash.png");
        var chosen = catalog.SelectedStrip;

        catalog.TakeImport(new SheetImportResult([], ["Fire.png: not a PNG image."]));

        Assert.Equal(["Burst.png", "Slash.png", "Gone.png"], catalog.Strips.Select(strip => strip.File));
        Assert.Same(chosen, catalog.SelectedStrip);
    }

    [Fact]
    public void TakeImport_ImportedRowTyped_RaisesTheProblem()
    {
        var catalog = Catalog();
        catalog.TakeImport(Imported("Fire.png", (24, 8)));
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.SelectedStrip!.HitFrameText = "9";

        Assert.Contains(nameof(EffectStripCatalogViewModel.Problem), notified);
    }

    [Fact]
    public void ImportLineOf_ImageCopied_SaysTheRecordsWaitForOk()
    {
        var line = EffectStripCatalogViewModel.ImportLineOf(new SheetImportResult(["Fire.png"], []));

        Assert.Equal(
            "Imported Fire.png. The images are in the effects folder now, never undone; a replaced strip starts again "
                + "from its record, and the records wait for OK.",
            line);
    }

    [Fact]
    public void ImportLineOf_NothingCopied_IsTheRefusalsAlone()
    {
        var line = EffectStripCatalogViewModel.ImportLineOf(new SheetImportResult([], ["Fire.png: not a PNG image."]));

        Assert.Equal("Refused: Fire.png: not a PNG image.", line);
    }

    private SheetImportResult Imported(string file, (int Width, int Height) size)
    {
        _sizes[file] = size;
        return new SheetImportResult([file], []);
    }

    private static Dictionary<string, TileAnimation> PaceOf(string file, int frameMilliseconds) =>
        new(StringComparer.Ordinal) { [file] = new TileAnimation { FrameCount = 3, FrameMilliseconds = frameMilliseconds } };

    private EffectStripCatalogViewModel Catalog() => new(
        _testWorld with
        {
            EffectStrips = new Dictionary<string, EffectStripDefinition> { ["Slash.png"] = _slash, ["Gone.png"] = _gone },
        },
        ["Slash.png", "Burst.png"],
        file => _sizes.TryGetValue(file, out var size) ? size : null);
}
