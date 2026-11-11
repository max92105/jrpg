using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A map's track in its settings file (decision 419): written when it has one, left out when it hasn't.
public sealed class ContentWriterMusicTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-music", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterMusicTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMaps_MapWithoutMusic_LeavesTheFieldOut()
    {
        var start = _catalog.Maps["start"];

        var edited = new MapDefinition(start.Layout, start.Settings with { MusicFile = null });

        _writer.WriteMaps(MapContent.With(_catalog, edited), [edited.Id]);

        Assert.DoesNotContain("musicFile", File.ReadAllText(_content.PathOf(Path.Combine("maps", "start.json"))), StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMaps_MapWithMusic_ReadsItBack()
    {
        var start = _catalog.Maps["start"];

        var edited = new MapDefinition(start.Layout, start.Settings with { MusicFile = "forest/loop.wav" });

        _writer.WriteMaps(MapContent.With(_catalog, edited), [edited.Id]);

        Assert.Equal("forest/loop.wav", ContentCatalogLoader.Load(_content.Path).Maps["start"].Settings.MusicFile);
    }
}
