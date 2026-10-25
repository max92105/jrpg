using Jrpg.Core.Effects;
using Jrpg.Core.Statuses;
using Jrpg.Data.Effects;

namespace Jrpg.Data.Tests.Effects;

public sealed class EffectValidatorTests
{
    [Fact]
    public void FindProblem_GrantStatusWithoutStatus_ReturnsProblem()
    {
        var problem = EffectValidator.FindProblem(new Effect(EffectKind.GrantStatus, Power: 0, Targeting.SingleEnemy));

        Assert.Equal("effect.status is required for a grantStatus effect.", problem);
    }

    [Fact]
    public void FindProblem_StatusWithZeroTurns_ReturnsProblem()
    {
        var bash = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 0));

        var problem = EffectValidator.FindProblem(bash);

        Assert.Equal("effect.status.turns must be at least 1 (was 0).", problem);
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
        var aura = new Effect(EffectKind.GrantStatus, 0, Targeting.AlliesInRange, new StatusGrant(StatusKind.Guarded, 5, 3), Range: -1);

        var problem = EffectValidator.FindProblem(aura);

        Assert.Equal("effect.range must be at least 0 (was -1).", problem);
    }

    [Fact]
    public void FindProblem_ReadyWithOtherTargeting_ReturnsProblem()
    {
        var problem = EffectValidator.FindProblem(new Effect(EffectKind.ReadyExtraTurn, Power: 0, Targeting.SingleEnemy));

        Assert.Equal("effect.targeting must be otherCombatant for a readyExtraTurn effect (was SingleEnemy).", problem);
    }

    [Fact]
    public void FindProblem_DamageWithStatus_ReturnsNull()
    {
        var bash = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 1));

        var problem = EffectValidator.FindProblem(bash);

        Assert.Null(problem);
    }
}
