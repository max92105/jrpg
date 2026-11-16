using Jrpg.Core.Effects;

namespace Jrpg.Core.Tests.Effects;

// A tiered effect's amount at a Power before modifiers and target (decision 1001), the battle's own formula.
public sealed class TieredEffectTests
{
    private static readonly TieredEffect _heal2 = new("heal_2", EffectKind.RestoreHp, 2);

    [Fact]
    public void AmountAt_PerPowerTwoAtPowerTen_IsTwenty()
    {
        var amount = _heal2.AmountAt(10);

        Assert.Equal(20, amount);
    }

    [Fact]
    public void AmountAt_ProductPastMaxPower_HeldAtMaxPower()
    {
        var amount = _heal2.AmountAt(DamageFormula.MaxPower);

        Assert.Equal(DamageFormula.MaxPower, amount);
    }

    // The upper bound's accepted side: a product exactly at the bound is kept.
    [Fact]
    public void AmountAt_ProductAtMaxPower_IsKept()
    {
        var amount = _heal2.AmountAt(DamageFormula.MaxPower / 2);

        Assert.Equal(DamageFormula.MaxPower, amount);
    }
}
