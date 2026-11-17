using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Backgrounds;
using static Jrpg.Editor.Tests.Backgrounds.BackgroundFixtures;

namespace Jrpg.Editor.Tests.Backgrounds;

// A PNG imported under the name of a record whose image was missing (decision 902), beside
// BattleBackgroundCatalogViewModelTests: the folder lists Pulse.png and A.png, and Gone.png's record has no image until
// the import copies one in.
public sealed class BattleBackgroundImportTests
{
    private static readonly AnimatedBackgroundDefinition _gone = Pulse with { File = "Gone.png" };

    [Fact]
    public void RefreshAfterStillImport_MissingImagesRecord_RowMovesAheadOfTheMissingOnes()
    {
        var catalog = CatalogWithGoneMissing();

        catalog.RefreshAfterStillImport([_gone.File]);

        Assert.Equal("A.png, Gone.png, Pulse.png", string.Join(", ", catalog.Backgrounds.Select(row => row.File)));
    }

    [Fact]
    public void RefreshAfterStillImport_MissingImagesRecord_DropsItsRecord()
    {
        var catalog = CatalogWithGoneMissing();

        catalog.RefreshAfterStillImport([_gone.File]);

        Assert.Equal([Pulse], catalog.ToRecords());
    }

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
