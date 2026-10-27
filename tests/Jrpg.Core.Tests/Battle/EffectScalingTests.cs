using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Tests.Battle;

public sealed class EffectScalingTests
{
    // 25 × 1.2 = 30; 23 × 1.2 = 27.6 rounds down to 27.
    [Theory]
    [InlineData(25, 30)]
    [InlineData(23, 27)]
    public void ScaledFor_TwentyPercentMore_RaisesDamage(int power, int expected)
    {
        var scaled = EffectScaling.ScaledFor(new Effect(EffectKind.Damage, power, Targeting.SingleEnemy), damagePermille: 1200);

        Assert.Equal(expected, scaled.Power);
    }

    [Fact]
    public void ScaledFor_Heal_RaisesHealing()
    {
        var scaled = EffectScaling.ScaledFor(new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly), damagePermille: 1200);

        Assert.Equal(18, scaled.Power);
    }

    [Fact]
    public void ScaledFor_GrantStatus_LeavesItAlone()
    {
        var poison = new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy, new StatusGrant(StatusKind.Poison, Power: 8, Turns: 3));

        var scaled = EffectScaling.ScaledFor(poison, damagePermille: 1200);

        Assert.Equal(poison, scaled);
    }
}
