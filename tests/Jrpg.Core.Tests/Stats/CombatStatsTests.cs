using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class CombatStatsTests
{
    [Fact]
    public void New_NothingGiven_HitsForCertain()
    {
        var stats = new CombatStats(AttackDamage: 10);

        Assert.Equal(1000, stats.AccuracyPermille);
        Assert.Equal(0, stats.DodgePermille);
    }
}
