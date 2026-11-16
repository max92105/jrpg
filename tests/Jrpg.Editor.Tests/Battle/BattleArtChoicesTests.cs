using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The three art folders as the Enemies page's boxes offer them: read through the shell's lookup, read again after an
// import, and the pictures an enemy names that its folder lacks (decision 442).
public sealed class BattleArtChoicesTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly Dictionary<string, List<string>> _folders = new(StringComparer.Ordinal)
    {
        [ArtFolders.Battlers] = ["Goblin.png", "Wolf.png"],
        [ArtFolders.Portraits] = ["Goblin.png", "WolfFace.png"],
        [ArtFolders.Effects] = ["AttackEnemy.png", "Bite.png"],
    };

    private static EnemyDefinition Wolf => _catalog.Enemies["goblin"] with
    {
        BattleSpriteFile = "Wolf.png",
        PortraitFile = "WolfFace.png",
        AttackEffectFile = "Bite.png",
    };

    [Fact]
    public void MissingOf_EveryPictureInItsFolder_IsEmpty()
    {
        var choices = new BattleArtChoices(folder => _folders[folder]);

        Assert.Empty(choices.MissingOf(EnemyViewModel.From(Wolf).Art));
    }

    [Fact]
    public void MissingOf_SpriteNotInFolder_MarksIt()
    {
        var choices = new BattleArtChoices(folder => _folders[folder]);
        var art = EnemyViewModel.From(Wolf with { BattleSpriteFile = "Bear.png" }).Art;

        Assert.Equal(["battle sprite Bear.png"], choices.MissingOf(art));
    }

    // A new enemy names no picture yet: each mark says so instead of ending in a blank.
    [Fact]
    public void MissingOf_NoPictureNamed_MarksEachAsNone()
    {
        var choices = new BattleArtChoices(folder => _folders[folder]);
        var art = EnemyViewModel.New().Art;

        Assert.Equal(["battle sprite (none)", "portrait (none)", "attack effect (none)"], choices.MissingOf(art));
    }

    [Fact]
    public void Refresh_AfterImport_ListsTheNewFileAndNotifies()
    {
        var battlers = new List<string> { "Goblin.png" };
        var choices = new BattleArtChoices(folder => folder == ArtFolders.Battlers ? [.. battlers] : []);
        var notified = new List<string?>();
        choices.PropertyChanged += (_, e) => notified.Add(e.PropertyName);
        battlers.Add("Wolf.png");

        choices.Refresh();

        Assert.Equal(["Goblin.png", "Wolf.png"], choices.Battlers);
        Assert.Contains(nameof(BattleArtChoices.Battlers), notified);
    }

    [Fact]
    public void ArtChoices_Catalog_ReadsTheThreeFolders()
    {
        var catalog = new EnemyCatalogViewModel(_catalog, folder => _folders[folder]);

        Assert.Equal(["Goblin.png", "Wolf.png"], catalog.ArtChoices.Battlers);
        Assert.Equal(["AttackEnemy.png", "Bite.png"], catalog.ArtChoices.Effects);
    }
}
