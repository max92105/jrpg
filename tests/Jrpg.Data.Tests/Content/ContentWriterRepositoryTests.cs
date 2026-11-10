using Jrpg.Core.Content;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// The writer keeps the repository's own files byte for byte: what the editor saves is what the developer committed. On
/// <c>content</c>, not the test world, since a rebuilt or new map must pass the day it is saved.
/// </summary>
public sealed class ContentWriterRepositoryTests : IDisposable
{
    private const string RepositoryFolder = "content";

    private readonly CopiedContentRoot _content = new("writer-repository", RepositoryFolder);
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterRepositoryTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    /// <summary>Every map in the repository, read from its folder so a new map is covered without an edit.</summary>
    public static TheoryData<string> RepositoryMapIds =>
        [.. Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, RepositoryFolder, "map-layouts"), "*.json")
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .Order(StringComparer.Ordinal)];

    public void Dispose() => _content.Dispose();

    [Theory]
    [MemberData(nameof(RepositoryMapIds))]
    public void WriteMap_UnchangedRepositoryMap_WritesIdenticalBytes(string mapId)
    {
        var filesBefore = MapFileBytes(mapId);

        _writer.WriteMap(_catalog.Maps[mapId], _catalog);

        Assert.Equal(filesBefore, MapFileBytes(mapId));
    }

    [Fact]
    public void WriteMapCatalogs_UnchangedRepositoryCatalogs_WritesIdenticalBytes()
    {
        var filesBefore = CatalogFileBytes();

        _writer.WriteMapCatalogs(_catalog);

        Assert.Equal(filesBefore, CatalogFileBytes());
    }

    // A map id rename writes the new game's file (decision 530): the repository's file must be laid out as the writer
    // lays it out, or a rename would rewrite its whitespace.
    [Fact]
    public void NewGame_RepositoryFile_WritesBackUnchanged()
    {
        var written = _content.PathOf("new-game.written.json");

        ContentFileWriter.Write(written, _catalog.NewGame);

        Assert.Equal(File.ReadAllBytes(_content.PathOf("new-game.json")), File.ReadAllBytes(written));
    }

    private byte[] CatalogFileBytes() =>
        [.. File.ReadAllBytes(_content.PathOf("tilesheets.json")),
         .. File.ReadAllBytes(_content.PathOf("tiles.json")),
         .. File.ReadAllBytes(_content.PathOf("objects.json")),
         .. File.ReadAllBytes(_content.PathOf("quests.json"))];

    // Both files one after the other, compared byte for byte: line endings and encoding count.
    private byte[] MapFileBytes(string mapId) =>
        [.. File.ReadAllBytes(Path.Combine(_content.Path, "map-layouts", $"{mapId}.json")),
         .. File.ReadAllBytes(Path.Combine(_content.Path, "maps", $"{mapId}.json"))];
}
