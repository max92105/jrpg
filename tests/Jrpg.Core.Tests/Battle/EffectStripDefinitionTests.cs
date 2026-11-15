using Jrpg.Core.Battle;

namespace Jrpg.Core.Tests.Battle;

// An effect strip's pace (ADR 0048): its record when listed, the unlisted 80 ms otherwise.
public sealed class EffectStripDefinitionTests
{
    private static readonly EffectStripDefinition _slash = new()
    {
        File = "GoldenSlashImpact.png",
        FrameMilliseconds = 17,
        HitFrame = 3,
        SwingFile = "GoldenSlashAttack.png",
    };

    [Fact]
    public void Of_Unlisted_PlaysAtTheUnlistedPaceWithNoSwing()
    {
        var strip = EffectStripDefinition.Of("AttackEnemy.png", new Dictionary<string, EffectStripDefinition>());

        Assert.Equal(
            ("AttackEnemy.png", TimeSpan.FromMilliseconds(80), 0, null),
            (strip.File, strip.FrameTime, strip.HitFrame, strip.SwingFile));
    }

    [Fact]
    public void Of_Listed_IsItsRecord()
    {
        var listed = new Dictionary<string, EffectStripDefinition> { [_slash.File] = _slash };

        Assert.Same(_slash, EffectStripDefinition.Of(_slash.File, listed));
    }

    [Fact]
    public void HitLead_HitFrameTwo_IsTwoFrameTimes()
    {
        var strip = new EffectStripDefinition { File = "Burst.png", FrameMilliseconds = 80, HitFrame = 2 };

        Assert.Equal(TimeSpan.FromMilliseconds(160), strip.HitLead);
    }

    [Fact]
    public void FrameTime_IsTheMilliseconds()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(17), _slash.FrameTime);
    }
}
