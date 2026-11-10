using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A map's folder in its settings file (decision 532): written when it has one, left out when it hasn't.
public sealed class ContentWriterFolderTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-folder", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterFolderTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    // The test world predates the field: its file writes back byte for byte.
    [Fact]
    public void WriteMap_NoFolder_LeavesTheFieldOut()
    {
        var start = _catalog.Maps["start"];
        var before = File.ReadAllBytes(SettingsPath("start"));

        _writer.WriteMap(start, _catalog);

        Assert.Equal(before, File.ReadAllBytes(SettingsPath("start")));
        Assert.DoesNotContain("folder", File.ReadAllText(SettingsPath("start")), StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMap_Folder_RoundTrips()
    {
        var start = _catalog.Maps["start"];

        _writer.WriteMap(new MapDefinition(start.Layout, start.Settings with { Folder = "Act 1/Village" }), _catalog);

        Assert.Equal("Act 1/Village", ContentCatalogLoader.Load(_content.Path).Maps["start"].Settings.Folder);
    }

    private string SettingsPath(string mapId) => _content.PathOf(Path.Combine("maps", $"{mapId}.json"));
}
