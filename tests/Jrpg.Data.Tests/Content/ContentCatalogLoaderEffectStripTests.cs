using Jrpg.Core.Battle;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// effect-strips.json is loaded with the catalog (ADR 0048), on a copy of the test world whose file the test writes.
public sealed class ContentCatalogLoaderEffectStripTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("effect-strips", "test-world");

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Load_ListedStrip_ReadsItsPaceAndSwing()
    {
        File.WriteAllText(_content.PathOf("effect-strips.json"), """
            [
              {
                "file": "Burst.png",
                "frameMilliseconds": 80,
                "hitFrame": 2,
                "swingFile": "Swing.png"
              }
            ]
            """);

        var strip = ContentCatalogLoader.Load(_content.Path).EffectStrips["Burst.png"];

        var expected = new EffectStripDefinition
        {
            File = "Burst.png",
            FrameMilliseconds = 80,
            HitFrame = 2,
            SwingFile = "Swing.png",
        };
        Assert.Equal(expected, strip);
    }

    [Fact]
    public void Load_NullEntry_NamesTheFile()
    {
        File.WriteAllText(_content.PathOf("effect-strips.json"), "[ null ]");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("effect-strips.json: entry 0 is null.", exception.Message);
    }

    [Fact]
    public void Load_RepositoryContent_Loads()
    {
        var exception = Record.Exception(() => ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));

        Assert.Null(exception);
    }
}
