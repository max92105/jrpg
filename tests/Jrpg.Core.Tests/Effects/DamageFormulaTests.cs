using Jrpg.Core.Effects;

namespace Jrpg.Core.Tests.Effects;

public sealed class DamageFormulaTests
{
    [Fact]
    public void Calculate_NothingInTheWay_LandsThePower()
    {
        var damage = DamageFormula.Calculate(power: 20, DamageFormula.NormalHit, Mitigation.None);

        Assert.Equal(20, damage);
    }

    // A quarter of 1 at the 75 % cap would round down to 0; every hit still counts.
    [Fact]
    public void Calculate_OnePowerAtCap_ReturnsMinimumDamage()
    {
        var damage = DamageFormula.Calculate(power: 1, DamageFormula.NormalHit, new Mitigation(ResistancePermille: 750, ExtraTakenPermille: 0));

        Assert.Equal(DamageFormula.MinimumDamage, damage);
    }

    [Fact]
    public void Calculate_Exposed50_RaisesByHalf()
    {
        var damage = DamageFormula.Calculate(power: 20, DamageFormula.NormalHit, new Mitigation(ResistancePermille: 0, ExtraTakenPermille: 500));

        Assert.Equal(30, damage);
    }

    [Fact]
    public void Calculate_FiftyPercentResistance_Halves()
    {
        var damage = DamageFormula.Calculate(power: 40, DamageFormula.NormalHit, new Mitigation(ResistancePermille: 500, ExtraTakenPermille: 0));

        Assert.Equal(20, damage);
    }

    [Fact]
    public void Calculate_NegativeResistance_RaisesDamage()
    {
        var damage = DamageFormula.Calculate(power: 40, DamageFormula.NormalHit, new Mitigation(ResistancePermille: -250, ExtraTakenPermille: 0));

        Assert.Equal(50, damage);
    }

    // 90 % is past the 75 % cap, so a quarter of the 40 still lands.
    [Fact]
    public void Calculate_ResistanceAboveCap_UsesCap()
    {
        var damage = DamageFormula.Calculate(power: 40, DamageFormula.NormalHit, new Mitigation(ResistancePermille: 900, ExtraTakenPermille: 0));

        Assert.Equal(10, damage);
    }

    // 41 × 0.75 × 1.5 = 46.125, rounded down once to 46; rounding after each share would give 30, then 45.
    [Fact]
    public void Calculate_ResistanceAndExposed_RoundsDownOnce()
    {
        var damage = DamageFormula.Calculate(power: 41, DamageFormula.NormalHit, new Mitigation(ResistancePermille: 250, ExtraTakenPermille: 500));

        Assert.Equal(46, damage);
    }

    // -150 % is past the -100 % floor, so the 40 is doubled, not more.
    [Fact]
    public void Calculate_WeaknessBelowFloor_UsesFloor()
    {
        var damage = DamageFormula.Calculate(power: 40, DamageFormula.NormalHit, new Mitigation(ResistancePermille: -1500, ExtraTakenPermille: 0));

        Assert.Equal(80, damage);
    }

    // Base crit damage (decision 300): half again.
    [Fact]
    public void Calculate_Crit150_RaisesHalf()
    {
        var damage = DamageFormula.Calculate(power: 20, critPermille: 1500, Mitigation.None);

        Assert.Equal(30, damage);
    }

    // 41 × 1.5 × 0.75 = 46.125, rounded down once to 46; rounding the crit first would give 61, then 45.
    [Fact]
    public void Calculate_CritThenResistance_RoundsDownOnce()
    {
        var damage = DamageFormula.Calculate(power: 41, critPermille: 1500, new Mitigation(ResistancePermille: 250, ExtraTakenPermille: 0));

        Assert.Equal(46, damage);
    }

    // The product of the four factors passes 2^31 (999 × 6495 × 2000 × 2000), so the long intermediate is what keeps
    // 999 × 6.495 × 2 × 2 = 25,954.02 right.
    [Fact]
    public void Calculate_Power999CritExposedWeakness_DoesNotOverflow()
    {
        var damage = DamageFormula.Calculate(power: 999, critPermille: 6495, new Mitigation(ResistancePermille: -1000, ExtraTakenPermille: 1000));

        Assert.Equal(25954, damage);
    }

    [Fact]
    public void AffinityOf_NegativeResistance_IsWeak()
    {
        Assert.Equal(HitAffinity.Weak, DamageFormula.AffinityOf(-1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(249)]
    public void AffinityOf_FromZeroToJustBelowThreshold_IsNeutral(int resistancePermille)
    {
        Assert.Equal(HitAffinity.Neutral, DamageFormula.AffinityOf(resistancePermille));
    }

    [Fact]
    public void AffinityOf_AtThreshold_IsResisted()
    {
        Assert.Equal(HitAffinity.Resisted, DamageFormula.AffinityOf(250));
    }

    // 37 × 10 % is 3.7: rounded down per hit (decision 365).
    [Fact]
    public void LifeStolen_TenPercentOfThirtySeven_IsThree()
    {
        Assert.Equal(3, DamageFormula.LifeStolen(37, 100));
    }

    [Fact]
    public void LifeStolen_NoLifeSteal_IsZero()
    {
        Assert.Equal(0, DamageFormula.LifeStolen(37, 0));
    }

    [Fact]
    public void PenetratedResistance_PositiveResistance_LosesItsPoints()
    {
        Assert.Equal(200, DamageFormula.PenetratedResistance(250, 50));
    }

    // Held at the 75 % cap before the points come off (decision 364): 670, not 750.
    [Fact]
    public void PenetratedResistance_AboveTheCap_CappedFirst()
    {
        Assert.Equal(670, DamageFormula.PenetratedResistance(900, 80));
    }

    [Fact]
    public void PenetratedResistance_SmallResistance_StopsAtZero()
    {
        Assert.Equal(0, DamageFormula.PenetratedResistance(30, 80));
    }

    [Fact]
    public void PenetratedResistance_Weakness_Unchanged()
    {
        Assert.Equal(-250, DamageFormula.PenetratedResistance(-250, 80));
    }

    [Fact]
    public void PenetratedResistance_NoPenetration_Unchanged()
    {
        Assert.Equal(250, DamageFormula.PenetratedResistance(250, 0));
    }
}
