using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The animated backgrounds' problem line (ADR 0059): the loader's own message for the edited records.
public sealed class CatalogProblemsAnimatedBackgroundTests
{
    private static readonly AnimatedBackgroundDefinition _pulse = new()
    {
        File = "ForestPulse.png",
        FrameWidth = 960,
        FrameHeight = 540,
        Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 200 },
    };

    [Fact]
    public void OfAnimatedBackgrounds_FrameHeightZero_NamesTheFileAndField()
    {
        var problem = CatalogProblems.OfAnimatedBackgrounds([_pulse with { FrameHeight = 0 }]);

        Assert.Equal(
            "animated-backgrounds.json: animated background 'ForestPulse.png': frameHeight must be at least 1 (was 0).",
            problem);
    }

    [Fact]
    public void OfAnimatedBackgrounds_ValidList_IsNone() => Assert.Null(CatalogProblems.OfAnimatedBackgrounds([_pulse]));
}
