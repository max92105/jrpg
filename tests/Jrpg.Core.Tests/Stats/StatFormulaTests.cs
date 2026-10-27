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
        DodgePer100Agility: 20,
        PhysicalResistancePer100Constitution: 25,
        MagicalResistancePer100Will: 25,
        StatusPercentPer100Intelligence: 100,
        IntelligencePerExtraStatusTurn: 25,
        UnarmedDamage: 18,
        UnarmedDamageType: DamageType.Blunt);

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
        Assert.Equal(40, StatFormula.DodgePermille(agility: 20, gearPercent: 0, _rules));
    }

    [Fact]
    public void DodgePermille_HighAgility_StopsAtCap()
    {
        Assert.Equal(500, StatFormula.DodgePermille(agility: 1000, gearPercent: 0, _rules));
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

    // 24 × 1.15 = 27.6, rounded down once.
    [Fact]
    public void Scaled_TwentyFourAtFifteenPercentMore_IsTwentySeven()
    {
        Assert.Equal(27, StatFormula.Scaled(24, damagePermille: 1150));
    }

    [Fact]
    public void WeaponDamage_SheetAtFifteenPercentMore_RaisesTheRawAttack()
    {
        var sheet = new StatSheet(
            PrimaryStats.None, MaxHp: 100, Speed: 10, AttackDamage: 24, AccuracyPermille: 1000, CritChancePermille: 0, CritDamagePermille: 1500,
            DodgePermille: 0, CrowdControlResistancePermille: 0, DamagePermille: 1150, Resistances.None, StatusPermille: 1000,
            ExtraStatusTurns: 0);

        Assert.Equal(27, StatFormula.WeaponDamage(sheet));
    }

    [Fact]
    public void WholePercentOf_NinetySevenPermille_IsNine()
    {
        Assert.Equal(9, StatFormula.WholePercentOf(97));
    }

    [Fact]
    public void WholePercentOf_MinusTwentyFiveAndAHalfPercent_IsMinusTwentySix()
    {
        Assert.Equal(-26, StatFormula.WholePercentOf(-255));
    }

    [Fact]
    public void PermilleOf_TwelvePercent_IsOneHundredTwenty()
    {
        Assert.Equal(120, StatFormula.PermilleOf(12));
    }

    [Fact]
    public void ResistancesFrom_TwentyConstitution_RaisesPhysicalOnly()
    {
        var resistances = StatFormula.ResistancesFrom(new PrimaryStats(0, 0, 0, Constitution: 20, Will: 0), Resistances.None, _rules);

        Assert.Equal(Resistances.ByGroup(physicalPermille: 50, magicalPermille: 0), resistances);
    }

    [Fact]
    public void ResistancesFrom_TwentyWill_RaisesMagicalOnly()
    {
        var resistances = StatFormula.ResistancesFrom(new PrimaryStats(0, 0, 0, Constitution: 0, Will: 20), Resistances.None, _rules);

        Assert.Equal(Resistances.ByGroup(physicalPermille: 0, magicalPermille: 50), resistances);
    }

    [Fact]
    public void ResistancesFrom_HugeConstitution_StopsAtCap()
    {
        var resistances = StatFormula.ResistancesFrom(new PrimaryStats(0, 0, 0, Constitution: 1000, Will: 0), Resistances.None, _rules);

        Assert.Equal(750, resistances.Of(DamageType.Slashing));
    }

    [Fact]
    public void ExtraStatusTurns_JustBelowBreakpoint_AddsNone()
    {
        Assert.Equal(0, StatFormula.ExtraStatusTurns(intelligence: 24, _rules));
    }

    [Fact]
    public void ExtraStatusTurns_AtBreakpoint_AddsOne()
    {
        Assert.Equal(1, StatFormula.ExtraStatusTurns(intelligence: 25, _rules));
    }

    [Fact]
    public void StatusPermille_TwentyIntelligence_IsOneHundredTwenty()
    {
        Assert.Equal(1200, StatFormula.StatusPermille(intelligence: 20, _rules));
    }

    [Fact]
    public void CrowdControlResistancePermille_Any_IsTheBase()
    {
        Assert.Equal(100, StatFormula.CrowdControlResistancePermille(_rules));
    }
}
