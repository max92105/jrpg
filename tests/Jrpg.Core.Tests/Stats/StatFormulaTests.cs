using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Stats;

public sealed class StatFormulaTests
{
    // design.md's placeholder: +1 % of the class's max HP per Constitution point.
    private static readonly StatRules _rules = new(HpPercentPer100Constitution: 100);

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
        Assert.Equal(220, StatFormula.MaxHp(classMaxHp: 200, constitution: 20, new StatRules(HpPercentPer100Constitution: 50)));
    }
}
