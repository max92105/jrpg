using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class PrimaryStatsTests
{
    [Fact]
    public void ValueOf_Will_ReadsWill()
    {
        var primaries = new PrimaryStats(Power: 1, Agility: 2, Intelligence: 3, Constitution: 4, Will: 5);

        Assert.Equal(5, primaries.ValueOf(StatKind.Will));
    }

    [Fact]
    public void ValueOf_NotAPrimary_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrimaryStats.None.ValueOf(StatKind.MaxHp));
    }
}
