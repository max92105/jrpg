using Jrpg.Core.Resources;

namespace Jrpg.Core.Tests.Resources;

public sealed class BackfireFormulaTests
{
    private static readonly BackfireRule _rule = new(ChancePerPointPercent: 5, DamagePercentOfMaxHp: 10);

    [Fact]
    public void ChancePercent_FourPoints_Is20()
    {
        var chance = BackfireFormula.ChancePercent(amount: 4, _rule);

        Assert.Equal(20, chance);
    }

    [Fact]
    public void ChancePercent_PastHundred_CapsAt100()
    {
        var chance = BackfireFormula.ChancePercent(amount: 30, _rule);

        Assert.Equal(100, chance);
    }

    [Fact]
    public void Damage_SmallMaxHp_AtLeastOne()
    {
        var damage = BackfireFormula.Damage(maxHp: 5, _rule);

        Assert.Equal(1, damage);
    }
}
