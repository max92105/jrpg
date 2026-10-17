using Jrpg.Core.Effects;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Effects;

public sealed class TargetRulesTests
{
    private readonly Vitals _living = new(hp: 50, maxHp: 100, mp: 0, maxMp: 0);
    private readonly Vitals _fallen = new(hp: 0, maxHp: 100, mp: 0, maxMp: 0);

    [Theory]
    [InlineData(Targeting.SingleEnemy, TargetRelation.Enemy, true)]
    [InlineData(Targeting.AllEnemies, TargetRelation.Enemy, true)]
    [InlineData(Targeting.SingleAlly, TargetRelation.Ally, true)]
    [InlineData(Targeting.SingleAlly, TargetRelation.Self, true)]
    [InlineData(Targeting.AllAllies, TargetRelation.Ally, true)]
    [InlineData(Targeting.Self, TargetRelation.Self, true)]
    [InlineData(Targeting.SingleAlly, TargetRelation.Enemy, false)]
    public void CanTarget_LivingCandidate_FollowsRelation(Targeting targeting, TargetRelation relation, bool expected)
    {
        var canTarget = TargetRules.CanTarget(targeting, relation, _living);

        Assert.Equal(expected, canTarget);
    }

    [Fact]
    public void CanTarget_SingleEnemyOnAlly_ReturnsFalse()
    {
        var canTarget = TargetRules.CanTarget(Targeting.SingleEnemy, TargetRelation.Ally, _living);

        Assert.False(canTarget);
    }

    [Fact]
    public void CanTarget_SelfOnOtherAlly_ReturnsFalse()
    {
        var canTarget = TargetRules.CanTarget(Targeting.Self, TargetRelation.Ally, _living);

        Assert.False(canTarget);
    }

    [Fact]
    public void CanTarget_SingleAllyOnFallenAlly_ReturnsFalse()
    {
        var canTarget = TargetRules.CanTarget(Targeting.SingleAlly, TargetRelation.Ally, _fallen);

        Assert.False(canTarget);
    }

    [Fact]
    public void CanTarget_SingleFallenAllyOnLivingAlly_ReturnsFalse()
    {
        var canTarget = TargetRules.CanTarget(Targeting.SingleFallenAlly, TargetRelation.Ally, _living);

        Assert.False(canTarget);
    }

    [Theory]
    [InlineData(Targeting.SingleFallenAlly, TargetRelation.Ally, true)]
    [InlineData(Targeting.AllFallenAllies, TargetRelation.Ally, true)]
    [InlineData(Targeting.AllFallenAllies, TargetRelation.Enemy, false)]
    [InlineData(Targeting.SingleEnemy, TargetRelation.Enemy, false)]
    public void CanTarget_FallenCandidate_OnlyFallenAllyTargetingsAccept(Targeting targeting, TargetRelation relation, bool expected)
    {
        var canTarget = TargetRules.CanTarget(targeting, relation, _fallen);

        Assert.Equal(expected, canTarget);
    }

    [Theory]
    [InlineData(Targeting.AllEnemies, true)]
    [InlineData(Targeting.AllAllies, true)]
    [InlineData(Targeting.AllFallenAllies, true)]
    [InlineData(Targeting.SingleEnemy, false)]
    [InlineData(Targeting.SingleAlly, false)]
    [InlineData(Targeting.Self, false)]
    [InlineData(Targeting.SingleFallenAlly, false)]
    public void HitsEveryValidTarget_Targeting_IsTrueOnlyForAllTargetings(Targeting targeting, bool expected)
    {
        var hitsEvery = TargetRules.HitsEveryValidTarget(targeting);

        Assert.Equal(expected, hitsEvery);
    }
}
