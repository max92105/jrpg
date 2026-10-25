using Jrpg.Core.Resources;

namespace Jrpg.Core.Tests.Resources;

public sealed class ReadinessFormulaTests
{
    [Fact]
    public void Charges_Speed30Per10_IsThree()
    {
        var charges = ReadinessFormula.Charges(speed: 30, speedPerCharge: 10);

        Assert.Equal(3, charges);
    }

    [Fact]
    public void Charges_SpeedBelowPer_IsOne()
    {
        var charges = ReadinessFormula.Charges(speed: 7, speedPerCharge: 10);

        Assert.Equal(1, charges);
    }
}
