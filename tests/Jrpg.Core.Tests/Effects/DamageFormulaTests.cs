using Jrpg.Core.Effects;

namespace Jrpg.Core.Tests.Effects;

public sealed class DamageFormulaTests
{
    [Fact]
    public void Calculate_PowerAboveDefense_ReturnsDifference()
    {
        var damage = DamageFormula.Calculate(power: 20, defense: 8);

        Assert.Equal(12, damage);
    }

    [Theory]
    [InlineData(8, 8)]
    [InlineData(5, 30)]
    public void Calculate_DefenseAtOrAbovePower_ReturnsMinimumDamage(int power, int defense)
    {
        var damage = DamageFormula.Calculate(power, defense);

        Assert.Equal(DamageFormula.MinimumDamage, damage);
    }

    // (30 - 10) = 20 gets through, and Exposed +50 % makes it 30; scaling the raw 30 first would give 35.
    [Fact]
    public void Calculate_Exposed50_MultipliesAfterDefense()
    {
        var damage = DamageFormula.Calculate(power: 30, defense: 10, damageTakenPercent: 150);

        Assert.Equal(30, damage);
    }

    [Fact]
    public void Calculate_ExposedOnMinimumHit_StaysAtLeastMinimum()
    {
        var damage = DamageFormula.Calculate(power: 5, defense: 20, damageTakenPercent: 150);

        Assert.Equal(DamageFormula.MinimumDamage, damage);
    }
}
