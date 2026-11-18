using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Battle;

// One hit's amount before its target, as the battle scales it (decision 993): the balance table's reading of
// EffectScaling, the damage modifiers' sum on damage only (decision 786).
public sealed class EffectAmountsTests
{
    private static readonly TieredEffect _fire = new("fire_1", EffectKind.Damage, 2);

    private static readonly TieredEffect _heal = new("heal_1", EffectKind.RestoreHp, 2);

    // Power 15 with a 120 % damage percent, which a tier ignores and a flat power follows (decision 784).
    private static readonly CombatStats _caster = new(0) { Power = 15, DamagePermille = 1200 };

    private static readonly CombatStats _weakenedCaster = _caster with { DamageModifierPermille = -250 };

    [Fact]
    public void PerHit_TieredDamage_PowerTimesPerPower()
    {
        var blast = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy) { Tier = _fire };

        Assert.Equal(30, EffectAmounts.PerHit(blast, _caster));
    }

    // 15 x 2 x 0.75 = 22.5, rounded down once.
    [Fact]
    public void PerHit_TieredDamageWithMinusTwentyFivePercent_ScaledByTheSum()
    {
        var blast = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy) { Tier = _fire };

        Assert.Equal(22, EffectAmounts.PerHit(blast, _weakenedCaster));
    }

    [Fact]
    public void PerHit_TieredHealWithAModifier_ModifierIgnored()
    {
        var heal = new Effect(EffectKind.RestoreHp, 0, Targeting.SingleAlly) { Tier = _heal };

        Assert.Equal(30, EffectAmounts.PerHit(heal, _weakenedCaster));
    }

    // 25 x 1.2 = 30.
    [Fact]
    public void PerHit_FlatDamage_RaisedByDamagePercent() =>
        Assert.Equal(30, EffectAmounts.PerHit(new Effect(EffectKind.Damage, 25, Targeting.SingleEnemy), _caster));

    // 10 x 1.2 = 12, the modifier left out as for any healing.
    [Fact]
    public void PerHit_FlatReviveWithAModifier_RaisedByDamagePercentOnly() =>
        Assert.Equal(12, EffectAmounts.PerHit(new Effect(EffectKind.Revive, 10, Targeting.SingleAlly), _weakenedCaster));

    [Theory]
    [InlineData(EffectKind.GrantStatus)]
    [InlineData(EffectKind.ReadyExtraTurn)]
    [InlineData(EffectKind.Rest)]
    public void PerHit_KindChangingNoHp_Throws(EffectKind kind) =>
        Assert.Throws<ArgumentException>(() => EffectAmounts.PerHit(new Effect(kind, 0, Targeting.Self), _caster));
}
