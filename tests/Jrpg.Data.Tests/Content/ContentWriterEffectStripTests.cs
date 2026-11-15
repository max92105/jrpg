using System.Text.Json.Nodes;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// effect-strips.json is written with the other catalogs (ADR 0048, decision 659): sorted by file, a hit frame at 0 and
// a missing swing left out, on a copy of the test world.
public sealed class ContentWriterEffectStripTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-effect-strips", "test-world");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterEffectStripTests()
    {
        _catalog = ContentCatalogLoader.Load(_content.Path);
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Write_StripWithHitFrameZero_LeavesItOut()
    {
        _writer.WriteMapCatalogs(With(Strip("Burst.png")));

        Assert.False(Written()[0]!.AsObject().ContainsKey("hitFrame"));
    }

    [Fact]
    public void Write_StripWithoutSwing_LeavesItOut()
    {
        _writer.WriteMapCatalogs(With(Strip("Burst.png")));

        Assert.False(Written()[0]!.AsObject().ContainsKey("swingFile"));
    }

    [Fact]
    public void Write_TwoStrips_SortsThemByFile()
    {
        _writer.WriteMapCatalogs(With(Strip("Slash.png"), Strip("Burst.png")));

        Assert.Equal(["Burst.png", "Slash.png"], Written().Select(strip => (string?)strip!["file"]));
    }

    [Fact]
    public void Write_StripsWithEveryField_LoadBackTheSame()
    {
        var slash = Strip("Slash.png") with { HitFrame = 3, SwingFile = "Swing.png" };

        _writer.WriteMapCatalogs(With(slash));

        Assert.Equal(slash, ContentCatalogLoader.Load(_content.Path).EffectStrips["Slash.png"]);
    }

    private static EffectStripDefinition Strip(string file) => new() { File = file, FrameMilliseconds = 60 };

    private ContentCatalog With(params EffectStripDefinition[] strips) =>
        _catalog with { EffectStrips = strips.ToDictionary(strip => strip.File, StringComparer.Ordinal) };

    private JsonArray Written() => JsonNode.Parse(File.ReadAllText(_content.PathOf("effect-strips.json")))!.AsArray();
}
