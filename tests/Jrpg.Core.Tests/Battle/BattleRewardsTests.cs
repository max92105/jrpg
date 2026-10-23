using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

public sealed class BattleRewardsTests
{
    private static readonly ExperienceTable _table = new([1000, 1000]);

    private static readonly EnemyDefinition[] _goblinTrio =
    [
        new EnemyDefinitionBuilder().Build(),
        new EnemyDefinitionBuilder().Build(),
        new EnemyDefinitionBuilder().Build(),
    ];

    [Fact]
    public void TotalExperience_ThreeGoblins_SumsTheirRewards()
    {
        var total = BattleRewards.TotalExperience(_goblinTrio);

        Assert.Equal(120, total);
    }

    [Fact]
    public void SharePerMember_ThreeWaySplit_DropsRemainder()
    {
        var share = BattleRewards.SharePerMember(totalExperience: 100, recipientCount: 3);

        Assert.Equal(33, share);
    }

    [Fact]
    public void SharePerMember_NoRecipients_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => BattleRewards.SharePerMember(totalExperience: 100, recipientCount: 0));
    }

    [Fact]
    public void GrantExperience_TwoLivingMembers_EachGainsShare()
    {
        var sarxian = new PartyMemberBuilder().WithId("sarxian").Build();
        var jazog = new PartyMemberBuilder().WithId("jazog").Build();

        var gains = BattleRewards.GrantExperience(_goblinTrio, [sarxian, jazog], _table);

        Assert.Equal([sarxian, jazog], gains.Select(gain => gain.Member));
        Assert.All(gains, gain => Assert.Equal(60, gain.Amount));
        Assert.Equal(60, sarxian.Experience);
        Assert.Equal(60, jazog.Experience);
    }

    [Fact]
    public void GrantExperience_UnevenSplit_DropsRemainder()
    {
        PartyMember[] members =
        [
            new PartyMemberBuilder().WithId("a").Build(),
            new PartyMemberBuilder().WithId("b").Build(),
            new PartyMemberBuilder().WithId("c").Build(),
            new PartyMemberBuilder().WithId("d").Build(),
            new PartyMemberBuilder().WithId("e").Build(),
            new PartyMemberBuilder().WithId("f").Build(),
            new PartyMemberBuilder().WithId("g").Build(),
        ];

        var gains = BattleRewards.GrantExperience(_goblinTrio, members, _table);

        // 120 / 7 = 17, and the remaining 1 is dropped.
        Assert.All(gains, gain => Assert.Equal(17, gain.Amount));
    }

    [Fact]
    public void GrantExperience_FallenMember_GainsNothing()
    {
        var sarxian = new PartyMemberBuilder().WithId("sarxian").Build();
        var jazog = new PartyMemberBuilder().WithId("jazog").Build();
        jazog.Vitals.TakeDamage(999);

        var gains = BattleRewards.GrantExperience(_goblinTrio, [sarxian, jazog], _table);

        var gain = Assert.Single(gains);
        Assert.Same(sarxian, gain.Member);
        Assert.Equal(120, gain.Amount);
        Assert.Equal(0, jazog.Experience);
    }

    [Fact]
    public void GrantExperience_EveryoneFallen_GrantsNothing()
    {
        var sarxian = new PartyMemberBuilder().Build();
        sarxian.Vitals.TakeDamage(999);

        var gains = BattleRewards.GrantExperience(_goblinTrio, [sarxian], _table);

        Assert.Empty(gains);
    }

    [Fact]
    public void GrantExperience_ShareCrossesThreshold_ReportsLevelSteps()
    {
        var sarxian = new PartyMemberBuilder().AtLevel(1, experience: 950).Build();

        var gain = Assert.Single(BattleRewards.GrantExperience(_goblinTrio, [sarxian], _table));

        Assert.Equal([new LevelProgressStep(1, 950, 1000, 1000), new LevelProgressStep(2, 0, 70, 1000)], gain.Steps);
    }

    [Fact]
    public void GrantExperience_MemberAtMaxLevel_GetsShareButNoSteps()
    {
        var veteran = new PartyMemberBuilder().AtLevel(3).Build();

        var gain = Assert.Single(BattleRewards.GrantExperience(_goblinTrio, [veteran], _table));

        Assert.Equal(120, gain.Amount);
        Assert.Empty(gain.Steps);
    }

    [Fact]
    public void GrantExperience_MemberAlreadyAtMaxLevel_IsAtMaxLevel()
    {
        var veteran = new PartyMemberBuilder().AtLevel(3).Build();

        var gain = Assert.Single(BattleRewards.GrantExperience(_goblinTrio, [veteran], _table));

        Assert.True(gain.IsAtMaxLevel);
    }

    // 950 + 120 passes the 1000 that level 2 needs, so the member ends at level 3, the table's highest.
    [Fact]
    public void GrantExperience_GainReachesMaxLevel_IsAtMaxLevel()
    {
        var sarxian = new PartyMemberBuilder().AtLevel(2, experience: 950).Build();

        var gain = Assert.Single(BattleRewards.GrantExperience(_goblinTrio, [sarxian], _table));

        Assert.Equal(3, sarxian.Level);
        Assert.True(gain.IsAtMaxLevel);
    }

    [Fact]
    public void GrantExperience_BelowMaxLevel_IsNotAtMaxLevel()
    {
        var sarxian = new PartyMemberBuilder().AtLevel(1).Build();

        var gain = Assert.Single(BattleRewards.GrantExperience(_goblinTrio, [sarxian], _table));

        Assert.False(gain.IsAtMaxLevel);
    }
}
