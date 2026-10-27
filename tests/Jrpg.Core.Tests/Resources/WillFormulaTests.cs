using Jrpg.Core.Resources;

namespace Jrpg.Core.Tests.Resources;

public sealed class WillFormulaTests
{
    [Fact]
    public void BonusPoints_TwentyFiveWillPerTen_IsTwo()
    {
        Assert.Equal(2, WillFormula.BonusPoints(will: 25, willPerPoint: 10));
    }

    // Gear that takes Will away takes points away once it passes a whole step, never for a part of one.
    [Fact]
    public void BonusPoints_MinusNineWillPerTen_IsZero()
    {
        Assert.Equal(0, WillFormula.BonusPoints(will: -9, willPerPoint: 10));
    }
}
