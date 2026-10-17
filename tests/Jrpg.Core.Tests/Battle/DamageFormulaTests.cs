using Jrpg.Core.Battle;

namespace Jrpg.Core.Tests.Battle;

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
}
