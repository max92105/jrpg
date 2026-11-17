using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Backgrounds;
using Jrpg.Editor.Tiles;
using static Jrpg.Editor.Tests.Backgrounds.BackgroundFixtures;

namespace Jrpg.Editor.Tests.Backgrounds;

// A PNG imported under the name of a record whose image was missing (decision 902), and a GIF's sheet imported with its
// record (decision 903), beside BattleBackgroundCatalogViewModelTests: the folder lists Pulse.png and A.png, and
// Gone.png's record has no image until the import copies one in. The GIF's record plays at 110 ms, not the pulse's 200.
public sealed class BattleBackgroundImportTests
{
    private static readonly AnimatedBackgroundDefinition _gone = Pulse with { File = "Gone.png" };

    private static readonly AnimatedBackgroundDefinition _fire =
        Pulse with { File = "Fire.png", Animation = Pulse.Animation with { FrameMilliseconds = 110 } };

    [Fact]
    public void TakeImport_MissingImagesRecord_RowMovesAheadOfTheMissingOnes()
    {
        var catalog = CatalogWithGoneMissing();

        catalog.TakeImport(new SheetImportResult([_gone.File], []));

        Assert.Equal("A.png, Gone.png, Pulse.png", string.Join(", ", catalog.Backgrounds.Select(row => row.File)));
    }

    [Fact]
    public void TakeImport_MissingImagesRecord_DropsItsRecord()
    {
        var catalog = CatalogWithGoneMissing();

        catalog.TakeImport(new SheetImportResult([_gone.File], []));

        Assert.Equal([Pulse], catalog.ToRecords());
    }

    [Fact]
    public void TakeImport_GifsSheetForANewFile_AddsAnAnimatedRowAndChoosesIt()
    {
        var catalog = CatalogWithGoneMissing();

        catalog.TakeImport(GifImported(_fire));

        var order = string.Join(", ", catalog.Backgrounds.Select(row => row.File));
        Assert.Equal(
            (_fire.File, "A.png, Fire.png, Pulse.png, Gone.png", "Animated: 4 frames of 960 x 540 px in a 2 x 2 grid"),
            (catalog.ChosenKey, order, catalog.SelectedBackground!.KindLine));
    }

    [Fact]
    public void TakeImport_GifsSheetOverAStill_PlaysAtTheRecordsTime()
    {
        var catalog = CatalogWithGoneMissing();

        catalog.TakeImport(GifImported(_fire with { File = "A.png" }));

        var chosen = catalog.SelectedBackground!;
        Assert.Equal(("A.png", true, "110"), (chosen.File, chosen.IsAnimated, chosen.FrameMillisecondsText));
    }

    [Fact]
    public void TakeImport_GifsSheet_IsAmongTheRecords()
    {
        var catalog = CatalogWithGoneMissing();

        catalog.TakeImport(GifImported(_fire));

        Assert.Equal([_fire, _gone, Pulse], catalog.ToRecords());
    }

    // What the import hands back for one GIF: its sheet copied in, with the record that describes it.
    private static SheetImportResult GifImported(AnimatedBackgroundDefinition record) =>
        new([record.File], []) { Backgrounds = new Dictionary<string, AnimatedBackgroundDefinition> { [record.File] = record } };

    // The lookup knows Gone.png's size from the start, as the folder holds it once the import ran: the list never asks
    // for a missing image's size, so only the import's row reads it.
    private static BattleBackgroundCatalogViewModel CatalogWithGoneMissing()
    {
        var testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
        ContentCatalog current = testWorld with
        {
            AnimatedBackgrounds = new Dictionary<string, AnimatedBackgroundDefinition>
            {
                [_gone.File] = _gone,
                [Pulse.File] = Pulse,
            },
        };
        return new BattleBackgroundCatalogViewModel(current, [Pulse.File, "A.png"], _ => PulseSheet);
    }
}
