using Jrpg.Core.Effects;

namespace Jrpg.Core.Tests.Effects;

public sealed class DamageFormulaTests
{
    [Fact]
    public void Calculate_PowerAboveDefense_ReturnsDifference()
    {
        var damage = DamageFormula.Calculate(power: 20, Defense(8));

        Assert.Equal(12, damage);
    }

    [Theory]
    [InlineData(8, 8)]
    [InlineData(5, 30)]
    public void Calculate_DefenseAtOrAbovePower_ReturnsMinimumDamage(int power, int defense)
    {
        var damage = DamageFormula.Calculate(power, Defense(defense));

        Assert.Equal(DamageFormula.MinimumDamage, damage);
    }

    // (30 - 10) = 20 gets through, and Exposed +50 % makes it 30; scaling the raw 30 first would give 35.
    [Fact]
    public void Calculate_Exposed50_MultipliesAfterDefense()
    {
        var damage = DamageFormula.Calculate(power: 30, new Mitigation(Defense: 10, ResistancePermille: 0, ExtraTakenPermille: 500));

        Assert.Equal(30, damage);
    }

    [Fact]
    public void Calculate_ExposedOnMinimumHit_StaysAtLeastMinimum()
    {
        var damage = DamageFormula.Calculate(power: 5, new Mitigation(Defense: 20, ResistancePermille: 0, ExtraTakenPermille: 500));

        Assert.Equal(DamageFormula.MinimumDamage, damage);
    }

    // (50 - 10) = 40 gets through and 50 % of it is taken off; halving the raw 50 first would give 15.
    [Fact]
    public void Calculate_FiftyPercentResistance_HalvesWhatGetsThrough()
    {
        var damage = DamageFormula.Calculate(power: 50, new Mitigation(Defense: 10, ResistancePermille: 500, ExtraTakenPermille: 0));

        Assert.Equal(20, damage);
    }

    [Fact]
    public void Calculate_NegativeResistance_RaisesDamage()
    {
        var damage = DamageFormula.Calculate(power: 40, new Mitigation(Defense: 0, ResistancePermille: -250, ExtraTakenPermille: 0));

        Assert.Equal(50, damage);
    }

    // 90 % is past the 75 % cap, so a quarter of the 40 still lands.
    [Fact]
    public void Calculate_ResistanceAboveCap_UsesCap()
    {
        var damage = DamageFormula.Calculate(power: 40, new Mitigation(Defense: 0, ResistancePermille: 900, ExtraTakenPermille: 0));

        Assert.Equal(10, damage);
    }

    // 41 × 0.75 × 1.5 = 46.125, rounded down once to 46; rounding after each share would give 30, then 45.
    [Fact]
    public void Calculate_ResistanceAndExposed_RoundsDownOnce()
    {
        var damage = DamageFormula.Calculate(power: 41, new Mitigation(Defense: 0, ResistancePermille: 250, ExtraTakenPermille: 500));

        Assert.Equal(46, damage);
    }

    // -150 % is past the -100 % floor, so the 40 is doubled, not more.
    [Fact]
    public void Calculate_WeaknessBelowFloor_UsesFloor()
    {
        var damage = DamageFormula.Calculate(power: 40, new Mitigation(Defense: 0, ResistancePermille: -1500, ExtraTakenPermille: 0));

        Assert.Equal(80, damage);
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

    private static Mitigation Defense(int defense) => Mitigation.None with { Defense = defense };
}
