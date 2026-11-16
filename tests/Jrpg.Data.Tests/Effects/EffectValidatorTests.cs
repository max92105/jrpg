using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Effects;

namespace Jrpg.Data.Tests.Effects;

public sealed class EffectValidatorTests
{
    private static readonly TieredEffect _fire1 = new("fire_1", EffectKind.Damage, 2) { DamageType = DamageType.Fire };

    public static TheoryData<Effect, string> InvalidTieredEffects => new()
    {
        {
            new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy)
            {
                Statuses = [new StatusGrant(StatusKind.Poison, 5, 2)],
                Tier = _fire1,
            },
            "effect.tier is only for a damage or restoreHp effect (was fire_1 on grantStatus)."
        },
        {
            new Effect(EffectKind.RestoreHp, 0, Targeting.SingleAlly) { Tier = _fire1 },
            "effect.kind is restoreHp but fire_1 is damage."
        },
        {
            new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy, DamageType: DamageType.Ice) { Tier = _fire1 },
            "effect.damageType is ice but fire_1 is fire."
        },
        {
            new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy, DamageType: DamageType.Fire) { Tier = _fire1 },
            "effect.power must be 0 beside a tiered effect, which sets the amount (was 5)."
        },
    };

    [Theory]
    [MemberData(nameof(InvalidTieredEffects))]
    public void FindProblem_InvalidTieredEffect_NamesIt(Effect effect, string problem)
    {
        Assert.Equal(problem, EffectValidator.FindProblem(effect));
    }

    public static TheoryData<StatusGrant[], string> InvalidStatusLists => new()
    {
        { [new StatusGrant(StatusKind.Poison, 5, 2), null!], "effect.statuses must not contain a null entry." },
        {
            [new StatusGrant(StatusKind.Poison, 5, 2), new StatusGrant(StatusKind.Poison, 8, 3)],
            "effect.statuses lists poison twice."
        },
        {
            [new StatusGrant(StatusKind.Poison, 5, 2), new StatusGrant(StatusKind.Exposed, 20, 0)],
            "effect.statuses[1].turns must be at least 1 (was 0)."
        },
    };

    [Theory]
    [MemberData(nameof(InvalidStatusLists))]
    public void FindProblem_InvalidStatusList_NamesIt(StatusGrant[] grants, string problem)
    {
        var hex = new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy) { Statuses = grants };

        Assert.Equal(problem, EffectValidator.FindProblem(hex));
    }

    [Fact]
    public void FindProblem_TwoDifferentStatuses_Accepted()
    {
        var hex = new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy)
        {
            Statuses = [new StatusGrant(StatusKind.Exposed, 20, 2), new StatusGrant(StatusKind.Poison, 5, 2)],
        };

        Assert.Null(EffectValidator.FindProblem(hex));
    }

    [Fact]
    public void FindProblem_TieredDamageMatchingItsRecord_Accepted()
    {
        var blast = new Effect(EffectKind.Damage, 0, Targeting.SingleEnemy, DamageType: DamageType.Fire) { Tier = _fire1 };

        Assert.Null(EffectValidator.FindProblem(blast));
    }

    [Fact]
    public void FindProblem_GrantStatusWithoutStatus_ReturnsProblem()
    {
        var problem = EffectValidator.FindProblem(new Effect(EffectKind.GrantStatus, Power: 0, Targeting.SingleEnemy));

        Assert.Equal("effect.statuses must list at least one status for a grantStatus effect.", problem);
    }

    [Fact]
    public void FindProblem_DamageWithoutType_NamesDamageType()
    {
        var problem = EffectValidator.FindProblem(new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy));

        Assert.Equal("effect.damageType is required for a damage effect.", problem);
    }

    [Fact]
    public void FindProblem_HealWithType_NamesDamageType()
    {
        var heal = new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly, DamageType: DamageType.Light);

        var problem = EffectValidator.FindProblem(heal);

        Assert.Equal("effect.damageType is only for a damage effect (was Light on RestoreHp).", problem);
    }

    [Fact]
    public void FindProblem_StatusWithZeroTurns_ReturnsProblem()
    {
        var bash = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, DamageType: DamageType.Blunt)
        {
            Statuses = [new StatusGrant(StatusKind.Stun, Power: 0, Turns: 0)],
        };

        var problem = EffectValidator.FindProblem(bash);

        Assert.Equal("effect.statuses[0].turns must be at least 1 (was 0).", problem);
    }

    [Fact]
    public void FindProblem_HitsOnHeal_ReturnsProblem()
    {
        var problem = EffectValidator.FindProblem(new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly, Hits: 2));

        Assert.Equal("effect.hits above 1 is only for a damage effect (was 2 on RestoreHp).", problem);
    }

    [Fact]
    public void FindProblem_ZeroHits_ReturnsProblem()
    {
        var problem = EffectValidator.FindProblem(new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, Hits: 0));

        Assert.Equal("effect.hits must be at least 1 (was 0).", problem);
    }

    [Fact]
    public void FindProblem_RangeOnOtherTargeting_ReturnsProblem()
    {
        var problem = EffectValidator.FindProblem(new Effect(EffectKind.RestoreHp, Power: 15, Targeting.AllAllies, Range: 2));

        Assert.Equal("effect.range is only for an alliesInRange effect (was 2 on AllAllies).", problem);
    }

    [Fact]
    public void FindProblem_NegativeRange_ReturnsProblem()
    {
        var aura = new Effect(EffectKind.GrantStatus, 0, Targeting.AlliesInRange, Range: -1)
        {
            Statuses = [new StatusGrant(StatusKind.Guarded, 5, 3)],
        };

        var problem = EffectValidator.FindProblem(aura);

        Assert.Equal("effect.range must be at least 0 (was -1).", problem);
    }

    [Fact]
    public void FindProblem_ReadyWithOtherTargeting_ReturnsProblem()
    {
        var problem = EffectValidator.FindProblem(new Effect(EffectKind.ReadyExtraTurn, Power: 0, Targeting.SingleEnemy));

        Assert.Equal("effect.targeting must be otherCombatant for a readyExtraTurn effect (was SingleEnemy).", problem);
    }

    // A stun's power is its chance to land (decision 324): 0 would never land, and past 100 it would read as more than
    // a sure stun.
    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void FindProblem_StunPowerOutsideChance_NamesThePower(int power)
    {
        var bash = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, DamageType: DamageType.Blunt)
        {
            Statuses = [new StatusGrant(StatusKind.Stun, power, Turns: 1)],
        };

        var problem = EffectValidator.FindProblem(bash);

        Assert.Equal($"effect.statuses[0].power must be within 1..100 for a stun, its chance to land (was {power}).", problem);
    }

    [Fact]
    public void FindProblem_DamageWithStatus_ReturnsNull()
    {
        var bash = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, DamageType: DamageType.Blunt)
        {
            Statuses = [new StatusGrant(StatusKind.Stun, Power: 60, Turns: 1)],
        };

        var problem = EffectValidator.FindProblem(bash);

        Assert.Null(problem);
    }
}
