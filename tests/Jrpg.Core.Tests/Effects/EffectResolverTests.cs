using Jrpg.Core.Effects;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Effects;

public sealed class EffectResolverTests
{
    [Fact]
    public void Apply_Damage_ReturnsDamageAfterDefense()
    {
        var target = new Vitals(hp: 40, maxHp: 40);

        var amount = EffectResolver.Apply(new Effect(EffectKind.Damage, Power: 20, Targeting.SingleEnemy), target, targetDefense: 2);

        Assert.Equal(18, amount);
        Assert.Equal(22, target.Hp);
    }

    [Fact]
    public void Apply_RestoreHpOnFullHpTarget_ReturnsZero()
    {
        var target = new Vitals(hp: 100, maxHp: 100);

        var amount = EffectResolver.Apply(new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly), target, targetDefense: 0);

        Assert.Equal(0, amount);
    }

    [Fact]
    public void Apply_ReviveOnFallenTarget_RestoresHp()
    {
        var target = new Vitals(hp: 0, maxHp: 100);

        var amount = EffectResolver.Apply(new Effect(EffectKind.Revive, Power: 15, Targeting.SingleFallenAlly), target, targetDefense: 0);

        Assert.Equal(15, amount);
        Assert.True(target.IsAlive);
    }
}
