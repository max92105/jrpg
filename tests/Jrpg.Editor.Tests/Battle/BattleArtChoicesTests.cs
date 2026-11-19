using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The three art folders as the Enemies and Party members pages' boxes offer them: read through the shell's lookup, read
// again after an import, and the pictures an enemy or a member names that its folder lacks (decision 442).
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

    private static EnemyDefinition Wolf => _catalog.Enemies["gnoll"] with
    {
        BattleSpriteFile = "Wolf.png",
        PortraitFile = "WolfFace.png",
        AttackEffectFile = "Bite.png",
    };

    [Fact]
    public void MissingNoteOf_EveryPictureInItsFolder_IsEmpty()
    {
        var choices = new BattleArtChoices(folder => _folders[folder]);

        Assert.Equal("", choices.MissingNoteOf(Wolf.BattleSpriteFile, Wolf.PortraitFile, Wolf.AttackEffectFile));
    }

    [Fact]
    public void MissingNoteOf_SpriteNotInFolder_NamesIt()
    {
        var choices = new BattleArtChoices(folder => _folders[folder]);

        var note = choices.MissingNoteOf("Bear.png", Wolf.PortraitFile, Wolf.AttackEffectFile);

        Assert.Equal("Not in the art folders: battle sprite Bear.png.", note);
    }

    // A new enemy or member names no picture yet: each mark says so instead of ending in a blank.
    [Fact]
    public void MissingNoteOf_NoPictureNamed_NamesEachAsNone()
    {
        var choices = new BattleArtChoices(folder => _folders[folder]);

        var note = choices.MissingNoteOf("", "", "");

        Assert.Equal("Not in the art folders: battle sprite (none), portrait (none), attack effect (none).", note);
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
