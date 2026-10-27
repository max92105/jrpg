using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class StatFormulaTests
{
    // design.md's placeholders: +1 % HP a Constitution point, base accuracy 90, base crit 5 %, +1 % damage a Power
    // point, +0.5 % crit damage a Power point, +1 % crit per 4 Agility, +1 % dodge per 5 Agility; crowd control
    // resistance 10 rather than design's 0, so its test reads a base that isn't 0.
    private static readonly StatRules _rules = new(
        HpPercentPer100Constitution: 100,
        BaseAccuracy: 90,
        BaseCritChance: 5,
        BaseDodge: 0,
        BaseCrowdControlResistance: 10,
        DamagePercentPer100Power: 100,
        CritDamagePercentPer100Power: 50,
        CritChancePer100Agility: 25,
        DodgePer100Agility: 20);

    [Fact]
    public void MaxHp_TwentyConstitution_AddsTwentyPercent()
    {
        Assert.Equal(240, StatFormula.MaxHp(classMaxHp: 200, constitution: 20, _rules));
    }

    [Fact]
    public void MaxHp_ZeroConstitution_IsClassHp()
    {
        Assert.Equal(220, StatFormula.MaxHp(classMaxHp: 220, constitution: 0, _rules));
    }

    [Fact]
    public void MaxHp_FractionalResult_RoundsDown()
    {
        // 220 × 1.14 = 250.8.
        Assert.Equal(250, StatFormula.MaxHp(classMaxHp: 220, constitution: 14, _rules));
    }

    [Fact]
    public void MaxHp_HalfRate_AddsHalfAPercentAPoint()
    {
        Assert.Equal(220, StatFormula.MaxHp(classMaxHp: 200, constitution: 20, _rules with { HpPercentPer100Constitution = 50 }));
    }

    [Fact]
    public void AccuracyPermille_FiveClassPoints_AddToTheBase()
    {
        Assert.Equal(950, StatFormula.AccuracyPermille(classAccuracy: 5, _rules));
    }

    [Fact]
    public void CritChancePermille_TwentyAgility_AddsFivePercent()
    {
        Assert.Equal(100, StatFormula.CritChancePermille(agility: 20, _rules));
    }

    [Fact]
    public void CritChancePermille_ThreeAgility_AddsSevenTenths()
    {
        // 0.75 % rounds down to 0.7 %: still a number that moves, the reason for tenths (ADR 0026).
        Assert.Equal(57, StatFormula.CritChancePermille(agility: 3, _rules));
    }

    [Fact]
    public void DodgePermille_BelowCap_FollowsAgility()
    {
        Assert.Equal(40, StatFormula.DodgePermille(agility: 20, _rules));
    }

    [Fact]
    public void DodgePermille_HighAgility_StopsAtCap()
    {
        Assert.Equal(StatFormula.DodgeCapPercent * 10, StatFormula.DodgePermille(agility: 1000, _rules));
    }

    [Fact]
    public void CritDamagePermille_ZeroPower_IsOneHundredFifty()
    {
        Assert.Equal(1500, StatFormula.CritDamagePermille(power: 0, _rules));
    }

    [Fact]
    public void CritDamagePermille_FortyPower_AddsTwenty()
    {
        Assert.Equal(1700, StatFormula.CritDamagePermille(power: 40, _rules));
    }

    [Fact]
    public void DamagePermille_TwentyPower_IsOneHundredTwenty()
    {
        Assert.Equal(1200, StatFormula.DamagePermille(power: 20, _rules));
    }

    [Fact]
    public void CrowdControlResistancePermille_Any_IsTheBase()
    {
        Assert.Equal(100, StatFormula.CrowdControlResistancePermille(_rules));
    }
}
