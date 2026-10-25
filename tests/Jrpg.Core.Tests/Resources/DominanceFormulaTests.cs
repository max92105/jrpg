using Jrpg.Core.Resources;

namespace Jrpg.Core.Tests.Resources;

public sealed class DominanceFormulaTests
{
    [Fact]
    public void Empowered_FiftyPercent_AddsHalf()
    {
        Assert.Equal(12, DominanceFormula.Empowered(8, 50));
    }

    [Fact]
    public void Empowered_OddPower_RoundsDown()
    {
        Assert.Equal(10, DominanceFormula.Empowered(7, 50));
    }

    [Fact]
    public void Empowered_ZeroPercent_Unchanged()
    {
        Assert.Equal(8, DominanceFormula.Empowered(8, 0));
    }
}
