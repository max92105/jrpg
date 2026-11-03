using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A tile's animation in tiles.json (ADR 0039): written when it has one, left out when it hasn't.
public sealed class ContentWriterAnimationTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-animation", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterAnimationTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_TileWithAnimation_ReadsBackEqual()
    {
        var tile = _catalog.Tiles.Values.First() with { Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 140 } };

        _writer.WriteMapCatalogs(_catalog with { Tiles = new Dictionary<int, TileDefinition>(_catalog.Tiles) { [tile.Id] = tile } });

        Assert.Equal(tile, ContentCatalogLoader.Load(_content.Path).Tiles[tile.Id]);
    }

    [Fact]
    public void WriteMapCatalogs_TileWithoutAnimation_LeavesTheFieldOut()
    {
        _writer.WriteMapCatalogs(_catalog);

        Assert.DoesNotContain("\"animation\"", File.ReadAllText(_content.PathOf("tiles.json")), StringComparison.Ordinal);
    }
}
