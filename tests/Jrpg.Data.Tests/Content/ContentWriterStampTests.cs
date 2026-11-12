using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// A save writes only the catalog files that changed since the editor read or wrote them, and refuses a changed one
/// edited on disk since (decision 559), on a copy of the test world; a hand edit is two appended bytes of valid JSON.
/// </summary>
public sealed class ContentWriterStampTests : IDisposable
{
    private const string HandEdit = "\r\n";

    private readonly CopiedContentRoot _content = new("writer-stamps", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterStampTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
        _writer.StampCatalogs(_catalog);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_OnlyObjectsChanged_WritesOnlyTheObjectsFile()
    {
        var before = Bytes("tiles.json", "tilesheets.json", "quests.json");
        var objectsBefore = Bytes("objects.json");

        _writer.WriteMapCatalogs(WithRenamedCategory());

        Assert.Equal(before, Bytes("tiles.json", "tilesheets.json", "quests.json"));
        Assert.NotEqual(objectsBefore, Bytes("objects.json"));
    }

    [Fact]
    public void WriteMapCatalogs_UnchangedFileEditedOnDisk_KeepsTheHandEdit()
    {
        File.AppendAllText(_content.PathOf("quests.json"), HandEdit);
        var quests = Bytes("quests.json");

        _writer.WriteMapCatalogs(WithRenamedCategory());

        Assert.Equal(quests, Bytes("quests.json"));
    }

    [Fact]
    public void WriteMapCatalogs_ChangedFileEditedOnDisk_RefusesNamingTheFileAndWritesNothing()
    {
        File.AppendAllText(_content.PathOf("objects.json"), HandEdit);
        var tile = _catalog.Tiles.Values.First();
        var tiles = new Dictionary<int, TileDefinition>(_catalog.Tiles);
        tiles[tile.Id] = tile with { IsWalkable = !tile.IsWalkable };
        var changed = WithRenamedCategory() with { Tiles = tiles };
        var before = Bytes("tiles.json", "objects.json");

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(changed));

        Assert.StartsWith("objects.json changed on disk since the editor read it", exception.Message, StringComparison.Ordinal);
        Assert.Equal(before, Bytes("tiles.json", "objects.json"));
    }

    // The stamps follow the write: the saved file is the new reference, so the next save skips it and a later hand edit
    // to it is kept.
    [Fact]
    public void WriteMapCatalogs_TwoSavesInARow_SecondSkipsTheWrittenFile()
    {
        var changed = WithRenamedCategory();
        _writer.WriteMapCatalogs(changed);
        File.AppendAllText(_content.PathOf("objects.json"), HandEdit);
        var objects = Bytes("objects.json");

        _writer.WriteMapCatalogs(changed);

        Assert.Equal(objects, Bytes("objects.json"));
    }

    [Fact]
    public void WriteMapCatalogs_HandLaidFileUnchanged_IsNotRelaid()
    {
        var questsPath = _content.PathOf("quests.json");
        File.WriteAllText(questsPath, File.ReadAllText(questsPath).Replace("\r\n", "\n", StringComparison.Ordinal));
        var writer = new ContentWriter(_content.Path);
        writer.StampCatalogs(ContentCatalogLoader.Load(_content.Path));
        var handLaid = Bytes("quests.json");

        writer.WriteMapCatalogs(WithRenamedCategory());

        Assert.Equal(handLaid, Bytes("quests.json"));
    }

    // Without the stamps every catalog is written, as before decision 559: the tests that never stamp count on it.
    [Fact]
    public void WriteMapCatalogs_NeverStamped_WritesEveryFile()
    {
        File.AppendAllText(_content.PathOf("quests.json"), HandEdit);
        var quests = Bytes("quests.json");

        new ContentWriter(_content.Path).WriteMapCatalogs(_catalog);

        Assert.NotEqual(quests, Bytes("quests.json"));
    }

    [Fact]
    public void WriteMapCatalogs_NothingChanged_WritesNothing()
    {
        string[] files = ["tilesheets.json", "tiles.json", "objects.json", "quests.json"];
        var writtenAt = files.Select(file => File.GetLastWriteTimeUtc(_content.PathOf(file))).ToList();

        _writer.WriteMapCatalogs(_catalog);

        Assert.Equal(writtenAt, files.Select(file => File.GetLastWriteTimeUtc(_content.PathOf(file))));
    }

    // A save whose moves failed partway leaves a file new on disk with its stamp old: the next save of the same content
    // passes it rather than calling it a hand edit.
    [Fact]
    public void WriteMapCatalogs_FileAlreadyHoldingTheBytes_IsNotRefused()
    {
        var changed = WithRenamedCategory();
        var writtenAhead = new ContentWriter(_content.Path);
        writtenAhead.WriteMapCatalogs(changed);

        _writer.WriteMapCatalogs(changed);

        Assert.Equal("Townsfolk", ContentCatalogLoader.Load(_content.Path).Objects["villager"].Category);
    }

    private ContentCatalog WithRenamedCategory()
    {
        var villager = _catalog.Objects["villager"] with { Category = "Townsfolk" };
        return _catalog with { Objects = new Dictionary<string, ObjectDefinition>(_catalog.Objects) { ["villager"] = villager } };
    }

    private byte[] Bytes(params string[] files) => [.. files.SelectMany(file => File.ReadAllBytes(_content.PathOf(file)))];
}
