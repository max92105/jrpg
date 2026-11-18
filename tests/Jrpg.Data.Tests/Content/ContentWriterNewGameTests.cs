using Jrpg.Core.Content;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// new-game.json is written with the catalogs (decision 961), on a copy of the test world: checked as the loader checks
// it, written only when it changed since the stamp, refused when edited on disk since, and checked by a map save too; a
// hand edit is two appended bytes.
public sealed class ContentWriterNewGameTests : IDisposable
{
    private const string FileName = "new-game.json";
    private const string HandEdit = "\r\n";

    private readonly CopiedContentRoot _content = new("writer-new-game", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterNewGameTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
        _writer.StampCatalogs(_catalog);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_ChangedGold_WritesNewGame()
    {
        var written = _catalog.NewGame with { StartingGold = _catalog.NewGame.StartingGold + 1 };

        _writer.WriteMapCatalogs(With(written));

        var reloaded = ContentCatalogLoader.Load(_content.Path).NewGame;
        Assert.Equal(written.StartingGold, reloaded.StartingGold);
        Assert.Equal(written.LeaderChoices, reloaded.LeaderChoices);
        Assert.Equal(written.StartingItems, reloaded.StartingItems);
        Assert.Equal(written.StartLocation, reloaded.StartLocation);
        Assert.Equal(written.FieldSpriteFile, reloaded.FieldSpriteFile);
    }

    // The hand edit kept proves the file wasn't rewritten while the shops' was.
    [Fact]
    public void WriteMapCatalogs_UnchangedNewGame_LeavesTheFile()
    {
        File.AppendAllText(_content.PathOf(FileName), HandEdit);
        var handEdited = File.ReadAllBytes(_content.PathOf(FileName));
        var store = _catalog.Shops["general_store"] with { Name = "Corner Store" };
        var shops = new Dictionary<string, ShopDefinition>(_catalog.Shops) { [store.Id] = store };

        _writer.WriteMapCatalogs(_catalog with { Shops = shops });

        Assert.Equal(handEdited, File.ReadAllBytes(_content.PathOf(FileName)));
        Assert.Equal("Corner Store", ContentCatalogLoader.Load(_content.Path).Shops["general_store"].Name);
    }

    [Fact]
    public void WriteMapCatalogs_LeaderNamingUnknownMember_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));
        var leaders = _catalog.NewGame.LeaderChoices.Append(new LeaderChoice { MemberId = "nobody" });

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(With(_catalog.NewGame with { LeaderChoices = [.. leaders] })));

        Assert.Equal("new-game.json: leaderChoices has unknown party member id 'nobody'.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    [Fact]
    public void WriteMapCatalogs_NewGameEditedOnDisk_Refuses()
    {
        File.AppendAllText(_content.PathOf(FileName), HandEdit);
        var handEdited = File.ReadAllBytes(_content.PathOf(FileName));

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(With(_catalog.NewGame with { StartingGold = _catalog.NewGame.StartingGold + 1 })));

        Assert.Equal(
            "new-game.json changed on disk since the editor read it: close the editor and reopen it to edit it, or undo the "
            + "changes to it.",
            exception.Message);
        Assert.Equal(handEdited, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    // The sprite's half of decision 1018: Save all checks the new game's sprite through the field sprites' check.
    [Fact]
    public void WriteMapCatalogs_NewGameSpriteWithoutRecord_WritesNothing()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogs(With(_catalog.NewGame with { FieldSpriteFile = "ghost.png" })));

        Assert.Equal("field-sprites.json: the new game names field sprite 'ghost.png', which has no record.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    // A map save checks the new game as the loader does, its sprite's record included.
    [Fact]
    public void WriteMaps_NewGameSpriteWithoutRecord_WritesNothing()
    {
        var layout = Path.Combine(_content.Path, "map-layouts", "intro.json");
        var before = File.ReadAllBytes(layout);

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMaps(With(_catalog.NewGame with { FieldSpriteFile = "ghost.png" }), ["intro"]));

        Assert.Equal("field-sprites.json: the new game names field sprite 'ghost.png', which has no record.", exception.Message);
        Assert.Equal(before, File.ReadAllBytes(layout));
    }

    private ContentCatalog With(NewGameDefinition newGame) => _catalog with { NewGame = newGame };
}
