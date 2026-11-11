using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// Whether a map shows the entry banner, in its settings file (decision 528): left out while true, written when off.
public sealed class ContentWriterBannerTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-banner", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterBannerTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    // The test world predates the field: every map shows the banner, and its file writes back byte for byte.
    [Fact]
    public void WriteMaps_ShowsBanner_LeavesTheFieldOut()
    {
        var start = _catalog.Maps["start"];
        var before = File.ReadAllBytes(SettingsPath("start"));

        _writer.WriteMaps(MapContent.With(_catalog, start), [start.Id]);

        Assert.Equal(before, File.ReadAllBytes(SettingsPath("start")));
        Assert.DoesNotContain("showsBanner", File.ReadAllText(SettingsPath("start")), StringComparison.Ordinal);
    }

    [Fact]
    public void Load_SettingsWithoutTheField_ShowsTheBanner() => Assert.True(_catalog.Maps["start"].Settings.ShowsBanner);

    [Fact]
    public void WriteMaps_BannerOff_WritesFalse()
    {
        var start = _catalog.Maps["start"];

        var edited = new MapDefinition(start.Layout, start.Settings with { ShowsBanner = false });

        _writer.WriteMaps(MapContent.With(_catalog, edited), [edited.Id]);

        Assert.Contains("\"showsBanner\": false", File.ReadAllText(SettingsPath("start")), StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMaps_BannerOff_RoundTrips()
    {
        var start = _catalog.Maps["start"];

        var edited = new MapDefinition(start.Layout, start.Settings with { ShowsBanner = false });

        _writer.WriteMaps(MapContent.With(_catalog, edited), [edited.Id]);

        Assert.False(ContentCatalogLoader.Load(_content.Path).Maps["start"].Settings.ShowsBanner);
    }

    private string SettingsPath(string mapId) => _content.PathOf(Path.Combine("maps", $"{mapId}.json"));
}
