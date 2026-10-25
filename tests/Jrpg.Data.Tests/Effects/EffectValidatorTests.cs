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
    public void FindProblem_DamageWithStatus_ReturnsNull()
    {
        var bash = new Effect(EffectKind.Damage, Power: 15, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 1));

        var problem = EffectValidator.FindProblem(bash);

        Assert.Null(problem);
    }
}
