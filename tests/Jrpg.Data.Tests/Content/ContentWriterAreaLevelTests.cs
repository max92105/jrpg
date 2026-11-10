using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A map's area level in its settings file (decision 527): written when it has one, left out when it hasn't.
public sealed class ContentWriterAreaLevelTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-area-level", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterAreaLevelTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    // The test world predates the field: its file writes back byte for byte.
    [Fact]
    public void WriteMap_NoAreaLevel_LeavesTheFieldOut()
    {
        var start = _catalog.Maps["start"];
        var before = File.ReadAllBytes(SettingsPath("start"));

        _writer.WriteMap(start, _catalog);

        Assert.Equal(before, File.ReadAllBytes(SettingsPath("start")));
        Assert.DoesNotContain("areaLevel", File.ReadAllText(SettingsPath("start")), StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMap_AreaLevel_RoundTrips()
    {
        var start = _catalog.Maps["start"];

        _writer.WriteMap(new MapDefinition(start.Layout, start.Settings with { AreaLevel = 7 }), _catalog);

        Assert.Equal(7, ContentCatalogLoader.Load(_content.Path).Maps["start"].Settings.AreaLevel);
    }

    private string SettingsPath(string mapId) => _content.PathOf(Path.Combine("maps", $"{mapId}.json"));
}
