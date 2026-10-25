using Jrpg.Core.Resources;

namespace Jrpg.Core.Tests.Resources;

public sealed class HeavyHitRuleTests
{
    [Fact]
    public void IsHeavy_ExactlyThreshold_ReturnsTrue()
    {
        var isHeavy = HeavyHitRule.IsHeavy(hpLost: 42, maxHp: 210, thresholdPercent: 20);

        Assert.True(isHeavy);
    }

    [Fact]
    public void IsHeavy_JustBelow_ReturnsFalse()
    {
        var isHeavy = HeavyHitRule.IsHeavy(hpLost: 41, maxHp: 210, thresholdPercent: 20);

        Assert.False(isHeavy);
    }
}
