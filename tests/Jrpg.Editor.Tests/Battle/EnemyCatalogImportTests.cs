using Jrpg.Core.Content;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Battle;

// After an Import... beside an art box, the Enemies window reads the folders again and picks the file copied last in
// that box for the chosen enemy (EN7's hand check: "it is picked").
public sealed class EnemyCatalogImportTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly List<string> _battlers = ["Goblin.png"];

    [Fact]
    public void TakeImport_BattlerCopied_RefreshesAndPicksItForTheChosenEnemy()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, folder => folder == ArtFolders.Battlers ? [.. _battlers] : []);
        _battlers.Add("Wolf.png");

        catalog.TakeImport(ArtFolders.Battlers, new SheetImportResult(["Wolf.png"], []));

        Assert.Contains("Wolf.png", catalog.ArtChoices.Battlers);
        Assert.Equal("Wolf.png", catalog.SelectedEnemy!.Art.BattleSpriteFile);
    }

    [Fact]
    public void TakeImport_EffectCopied_PicksItInTheEffectBox()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var portrait = catalog.SelectedEnemy!.Art.PortraitFile;

        catalog.TakeImport(ArtFolders.Effects, new SheetImportResult(["Bite.png"], []));

        var art = catalog.SelectedEnemy.Art;
        Assert.Equal(("Bite.png", portrait), (art.AttackEffectFile, art.PortraitFile));
    }

    // A refused import picks nothing: the box keeps what it had.
    [Fact]
    public void TakeImport_NothingCopied_LeavesTheBoxes()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, _ => []);
        var sprite = catalog.SelectedEnemy!.Art.BattleSpriteFile;

        catalog.TakeImport(ArtFolders.Battlers, new SheetImportResult([], ["Wolf.png: not a PNG image."]));

        Assert.Equal(sprite, catalog.SelectedEnemy.Art.BattleSpriteFile);
    }

    [Fact]
    public void Pick_FolderOfNoEnemyPicture_Throws()
    {
        var art = EnemyViewModel.From(_catalog.Enemies["goblin"]).Art;

        Assert.Throws<ArgumentException>(() => art.Pick(ArtFolders.Tilesheets, "Grass.png"));
    }
}
