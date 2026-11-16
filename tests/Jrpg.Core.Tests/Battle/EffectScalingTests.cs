using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;

namespace Jrpg.Core.Tests.Battle;

public sealed class EffectScalingTests
{
    private static readonly CombatStats TwentyPercentMorePower = new(0) { DamagePermille = 1200 };

    // 25 × 1.2 = 30; 23 × 1.2 = 27.6 rounds down to 27.
    [Theory]
    [InlineData(25, 30)]
    [InlineData(23, 27)]
    public void ScaledFor_TwentyPercentMore_RaisesDamage(int power, int expected)
    {
        var scaled = EffectScaling.ScaledFor(new Effect(EffectKind.Damage, power, Targeting.SingleEnemy), TwentyPercentMorePower);

        Assert.Equal(expected, scaled.Power);
    }

    [Fact]
    public void ScaledFor_Heal_RaisesHealing()
    {
        var scaled = EffectScaling.ScaledFor(new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly), TwentyPercentMorePower);

        Assert.Equal(18, scaled.Power);
    }

    // Power is magnitude, Intelligence is shape (decision 296): a grant follows the status percent, not the damage one.
    [Fact]
    public void ScaledFor_GrantStatus_FollowsTheStatusPercentNotTheDamageOne()
    {
        var poison = new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy, new StatusGrant(StatusKind.Poison, Power: 8, Turns: 3));

        var scaled = EffectScaling.ScaledFor(poison, TwentyPercentMorePower);

        Assert.Equal(poison, scaled);
    }

    [Fact]
    public void ScaledFor_GrantByCleverUser_ShapesTheGrant()
    {
        var poison = new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy, new StatusGrant(StatusKind.Poison, Power: 10, Turns: 3));

        var scaled = EffectScaling.ScaledFor(poison, new CombatStats(0) { StatusPermille = 1500, ExtraStatusTurns = 1 });

        Assert.Equal(new StatusGrant(StatusKind.Poison, Power: 15, Turns: 4), scaled.Status);
    }

    // Decision 784: Power 20 at 200 % damage and a tiered record of 4 per Power give 80; Power's percent would count
    // Power a second time.
    [Fact]
    public void ScaledFor_TieredDamage_IsPowerTimesPerPower()
    {
        var tier = new TieredEffect("fire_2", EffectKind.Damage, 4);
        var blast = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy) { Tier = tier };

        var scaled = EffectScaling.ScaledFor(blast, StrongCaster);

        Assert.Equal(80, scaled.Power);
    }

    [Fact]
    public void ScaledFor_TieredHeal_IsPowerTimesPerPower()
    {
        var tier = new TieredEffect("heal_1", EffectKind.RestoreHp, 2);
        var heal = new Effect(EffectKind.RestoreHp, 0, Targeting.SingleAlly) { Tier = tier };

        var scaled = EffectScaling.ScaledFor(heal, StrongCaster);

        Assert.Equal(40, scaled.Power);
    }

    // The other side: without a tier, Power's percent still raises the flat value.
    [Fact]
    public void ScaledFor_FlatDamageByStrongCaster_IsRaisedByDamagePercent()
    {
        var blast = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy);

        Assert.Equal(20, EffectScaling.ScaledFor(blast, StrongCaster).Power);
    }

    private static CombatStats StrongCaster => new(0) { Power = 20, DamagePermille = 2000 };
}
