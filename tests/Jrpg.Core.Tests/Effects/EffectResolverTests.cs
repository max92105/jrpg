using Jrpg.Core.Effects;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Effects;

public sealed class EffectResolverTests
{
    [Fact]
    public void Apply_Damage_ReturnsTheHpLost()
    {
        var target = new Vitals(hp: 40, maxHp: 40);

        var amount = EffectResolver.Apply(new Effect(EffectKind.Damage, Power: 20, Targeting.SingleEnemy), target, Mitigation.None);

        Assert.Equal(20, amount);
        Assert.Equal(20, target.Hp);
    }

    [Fact]
    public void Apply_RestoreHpOnFullHpTarget_ReturnsZero()
    {
        var target = new Vitals(hp: 100, maxHp: 100);

        var amount = EffectResolver.Apply(new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly), target, Mitigation.None);

        Assert.Equal(0, amount);
    }

    [Fact]
    public void Apply_ReviveOnFallenTarget_RestoresHp()
    {
        var target = new Vitals(hp: 0, maxHp: 100);

        var amount = EffectResolver.Apply(new Effect(EffectKind.Revive, Power: 15, Targeting.SingleFallenAlly), target, Mitigation.None);

        Assert.Equal(15, amount);
        Assert.True(target.IsAlive);
    }
}
