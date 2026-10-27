using Jrpg.Core.Effects;

namespace Jrpg.Core.Tests.Effects;

public sealed class HitFormulaTests
{
    [Fact]
    public void ChancePermille_AccuracyAboveDodge_IsTheDifference()
    {
        var chance = HitFormula.ChancePermille(accuracyPermille: 900, dodgePermille: 50);

        Assert.Equal(850, chance);
    }

    [Fact]
    public void ChancePermille_AccuracyBelowDodge_IsFloor()
    {
        var chance = HitFormula.ChancePermille(accuracyPermille: 800, dodgePermille: 900);

        Assert.Equal(200, chance);
    }

    [Fact]
    public void ChancePermille_AccuracyFarAboveDodge_IsCeiling()
    {
        var chance = HitFormula.ChancePermille(accuracyPermille: 1200, dodgePermille: 50);

        Assert.Equal(1000, chance);
    }
}
