using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// An object tile's level in objects.json (ADR 0040): written above the base, left out on it.
public sealed class ContentWriterObjectLevelTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-object-level", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterObjectLevelTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_ObjectWithALevelOneTile_ReadsBackEqual()
    {
        var original = _catalog.Objects.Values.First(definition => definition.Tiles.Count > 0);
        var baseTile = original.Tiles[0];
        var stacked = original with { Tiles = [.. original.Tiles, baseTile with { Level = 1 }] };

        _writer.WriteMapCatalogs(_catalog with { Objects = new Dictionary<string, ObjectDefinition>(_catalog.Objects) { [stacked.Id] = stacked } });

        Assert.Equal(stacked.Tiles, ContentCatalogLoader.Load(_content.Path).Objects[stacked.Id].Tiles);
    }

    [Fact]
    public void WriteMapCatalogs_ObjectWithALevelUnderTheBase_ReadsBackEqual()
    {
        var original = _catalog.Objects.Values.First(definition => definition.Tiles.Count > 0);
        var stacked = original with { Tiles = [original.Tiles[0] with { Level = -1 }, .. original.Tiles] };

        _writer.WriteMapCatalogs(_catalog with { Objects = new Dictionary<string, ObjectDefinition>(_catalog.Objects) { [stacked.Id] = stacked } });

        Assert.Equal(stacked.Tiles, ContentCatalogLoader.Load(_content.Path).Objects[stacked.Id].Tiles);
    }

    [Fact]
    public void WriteMapCatalogs_BaseLevelTiles_LeaveTheFieldOut()
    {
        _writer.WriteMapCatalogs(_catalog);

        Assert.DoesNotContain("\"level\"", File.ReadAllText(_content.PathOf("objects.json")), StringComparison.Ordinal);
    }
}
