using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Effects;

namespace Jrpg.Data.Tests.Effects;

public sealed class EffectValidatorTests
{
    private static readonly TieredEffect _fire1 = new("fire_1", EffectKind.Damage, 2) { DamageType = DamageType.Fire };
    private static readonly StatusDefinition _poison = Record("poison", StatusPartKind.DamageEachTurn, 5);
    private static readonly StatusDefinition _exposed = Record("exposed", StatusPartKind.DamageTakenChange, 20);
    private static readonly StatusDefinition _guard = Record("guard", StatusPartKind.ResistanceChange, 5);
    private static readonly StatusDefinition _stun = Record("stun", StatusPartKind.SkipTurn, 0);

    public static TheoryData<Effect, string> InvalidTieredEffects => new()
    {
        {
            new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy)
            {
                Statuses = [new StatusGrant(_poison, 2)],
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
        { [new StatusGrant(_poison, 2), null!], "effect.statuses must not contain a null entry." },
        {
            [new StatusGrant(_poison, 2), new StatusGrant(_poison, 3)],
            "effect.statuses lists poison twice."
        },
        {
            [new StatusGrant(_poison, 2), new StatusGrant(_exposed, 0)],
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
            Statuses = [new StatusGrant(_exposed, 2), new StatusGrant(_poison, 2)],
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
            Statuses = [new StatusGrant(_stun, Turns: 0)],
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
            Statuses = [new StatusGrant(_guard, 3)],
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

    // A grant's chance to land is a percent (decision 798): at 0 it would never land, and a number past 100 would mean
    // nothing more than 100 does.
    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void FindProblem_ChanceOutsideAPercent_NamesTheChance(int chance)
    {
        var problem = EffectValidator.FindProblem(BashWithChance(chance));

        Assert.Equal($"effect.statuses[0].chance must be within 1..100 (was {chance}).", problem);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void FindProblem_ChanceAtABound_Accepted(int chance)
    {
        Assert.Null(EffectValidator.FindProblem(BashWithChance(chance)));
    }

    [Fact]
    public void FindProblem_DamageWithStatus_ReturnsNull()
    {
        var bash = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, DamageType: DamageType.Blunt)
        {
            Statuses = [new StatusGrant(_stun, Turns: 1) { Chance = 60 }],
        };

        var problem = EffectValidator.FindProblem(bash);

        Assert.Null(problem);
    }

    private static Effect BashWithChance(int chance) =>
        new(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, DamageType: DamageType.Blunt)
        {
            Statuses = [new StatusGrant(_stun, Turns: 1) { Chance = chance }],
        };

    private static StatusDefinition Record(string id, StatusPartKind kind, int amount) =>
        new() { Id = id, Name = id, Description = "", Parts = [new StatusPart(kind, amount)] };
}
