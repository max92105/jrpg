using System.Text.Json.Nodes;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// animated-backgrounds.json is written with the other catalogs (ADR 0059): checked as the loader checks it, sorted by
// file, on a copy of the test world.
public sealed class ContentWriterAnimatedBackgroundTests : IDisposable
{
    private const string FileName = "animated-backgrounds.json";

    private readonly CopiedContentRoot _content = new("writer-animated-backgrounds", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterAnimatedBackgroundTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogsAndMaps_ARecordAdded_WritesItSortedByFile()
    {
        _writer.WriteMapCatalogsAndMaps(With(Background("Waves.png"), Background("Embers.png")), []);

        Assert.Equal(["Embers.png", "Waves.png"], Written().Select(background => (string?)background!["file"]));
    }

    [Fact]
    public void WriteMapCatalogsAndMaps_InvalidRecord_IsRefusedAndNothingWritten()
    {
        var before = File.ReadAllBytes(_content.PathOf(FileName));

        var exception = Assert.Throws<ContentValidationException>(
            () => _writer.WriteMapCatalogsAndMaps(With(Background("Waves.png") with { FrameWidth = 0 }), []));

        Assert.Equal(
            "animated-backgrounds.json: animated background 'Waves.png': frameWidth must be at least 1 (was 0).",
            exception.Message);
        Assert.Equal(before, File.ReadAllBytes(_content.PathOf(FileName)));
    }

    private static AnimatedBackgroundDefinition Background(string file) => new()
    {
        File = file,
        FrameWidth = 480,
        FrameHeight = 270,
        Animation = new TileAnimation { FrameCount = 8, FrameMilliseconds = 100 },
    };

    private ContentCatalog With(params AnimatedBackgroundDefinition[] backgrounds) =>
        _catalog with { AnimatedBackgrounds = backgrounds.ToDictionary(background => background.File, StringComparer.Ordinal) };

    private JsonArray Written() => JsonNode.Parse(File.ReadAllText(_content.PathOf(FileName)))!.AsArray();
}
