using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class StatBonusesTests
{
    [Fact]
    public void Of_NamedKind_ReadsItsBonus()
    {
        var bonuses = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 3 });

        Assert.Equal(3, bonuses.Of(StatKind.Will));
    }

    [Fact]
    public void Of_KindNotNamed_IsZero()
    {
        var bonuses = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 3 });

        Assert.Equal(0, bonuses.Of(StatKind.Power));
    }

    [Fact]
    public void Equals_SameBonusesInAnotherOrder_IsTrue()
    {
        var first = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 3, [StatKind.FireResistance] = 10 });
        var second = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.FireResistance] = 10, [StatKind.Will] = 3 });

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Equals_ADifferentBonus_IsFalse()
    {
        var first = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 3 });
        var second = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 4 });

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Plus_TwoItems_AddsKindByKind()
    {
        var ring = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 3, [StatKind.Power] = 2 });
        var amulet = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 4 });

        var both = ring.Plus(amulet);

        Assert.Equal(new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 7, [StatKind.Power] = 2 }), both);
    }

    [Fact]
    public void Plus_BonusesThatCancelOut_IsNone()
    {
        var blessed = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = 3 });
        var cursed = new StatBonuses(new Dictionary<StatKind, int> { [StatKind.Will] = -3 });

        Assert.Equal(StatBonuses.None, blessed.Plus(cursed));
    }

    [Fact]
    public void Constructor_CallersDictionaryChangedAfter_KeepsTheBonuses()
    {
        var values = new Dictionary<StatKind, int> { [StatKind.Will] = 3 };
        var bonuses = new StatBonuses(values);

        values[StatKind.Will] = 9;

        Assert.Equal(3, bonuses.Of(StatKind.Will));
    }
}
