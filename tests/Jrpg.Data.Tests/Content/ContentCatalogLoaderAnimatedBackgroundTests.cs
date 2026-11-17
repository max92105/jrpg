using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// animated-backgrounds.json is loaded with the catalog (ADR 0059), on a copy of the test world the test writes it in.
public sealed class ContentCatalogLoaderAnimatedBackgroundTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("animated-backgrounds", "test-world");

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Load_FileWithARecord_ReadsItByFile()
    {
        File.WriteAllText(_content.PathOf("animated-backgrounds.json"), """
            [
              {
                "file": "ForestPulse.png",
                "frameWidth": 960,
                "frameHeight": 540,
                "animation": {
                  "frameCount": 4,
                  "frameMilliseconds": 200
                }
              }
            ]
            """);

        var background = ContentCatalogLoader.Load(_content.Path).AnimatedBackgrounds["ForestPulse.png"];

        var expected = new AnimatedBackgroundDefinition
        {
            File = "ForestPulse.png",
            FrameWidth = 960,
            FrameHeight = 540,
            Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 200 },
        };
        Assert.Equal(expected, background);
    }
}
