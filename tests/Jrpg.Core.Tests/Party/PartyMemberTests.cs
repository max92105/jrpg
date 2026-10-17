using Jrpg.Core.Progression;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class PartyMemberTests
{
    private readonly ExperienceTable _table = new([100, 120, 144]);

    [Fact]
    public void Recruit_Definition_CopiesStartingValues()
    {
        var member = new PartyMemberBuilder()
            .AtLevel(2, experience: 30)
            .WithVitals(hp: 40, maxHp: 100, mp: 5, maxMp: 20)
            .WithStats(attack: 12, defense: 7)
            .Build();

        Assert.Equal(2, member.Level);
        Assert.Equal(30, member.Experience);
        Assert.Equal(new CombatStats(Attack: 12, Defense: 7), member.BaseStats);
        Assert.Equal((40, 100, 5, 20), (member.Vitals.Hp, member.Vitals.MaxHp, member.Vitals.Mp, member.Vitals.MaxMp));
    }

    [Fact]
    public void GainExperience_BelowThreshold_ReturnsOneStepWithoutLevelUp()
    {
        var member = new PartyMemberBuilder().AtLevel(1, experience: 20).Build();

        var steps = member.GainExperience(30, _table);

        Assert.Equal([new LevelProgressStep(1, 20, 50, 100)], steps);
        Assert.False(steps[0].IsLevelUp);
        Assert.Equal(1, member.Level);
        Assert.Equal(50, member.Experience);
    }

    [Fact]
    public void GainExperience_ExactThreshold_LevelsUpWithZeroCarryOver()
    {
        var member = new PartyMemberBuilder().AtLevel(1, experience: 60).Build();

        var steps = member.GainExperience(40, _table);

        Assert.Equal([new LevelProgressStep(1, 60, 100, 100)], steps);
        Assert.True(steps[0].IsLevelUp);
        Assert.Equal(2, member.Level);
        Assert.Equal(0, member.Experience);
    }

    [Fact]
    public void GainExperience_CrossesTwoThresholds_ReturnsOneStepPerLevel()
    {
        var member = new PartyMemberBuilder().AtLevel(1, experience: 90).Build();

        var steps = member.GainExperience(140, _table);

        LevelProgressStep[] expected =
        [
            new(1, 90, 100, 100),
            new(2, 0, 120, 120),
            new(3, 0, 10, 144),
        ];
        Assert.Equal(expected, steps);
        Assert.Equal(3, member.Level);
        Assert.Equal(10, member.Experience);
    }

    [Fact]
    public void GainExperience_ReachesMaxLevel_StopsAndDropsTheRest()
    {
        var member = new PartyMemberBuilder().AtLevel(3, experience: 140).Build();

        var steps = member.GainExperience(50, _table);

        Assert.Equal([new LevelProgressStep(3, 140, 144, 144)], steps);
        Assert.Equal(4, member.Level);
        Assert.Equal(0, member.Experience);
    }

    [Fact]
    public void GainExperience_AtMaxLevel_ReturnsNoStepsAndKeepsState()
    {
        var member = new PartyMemberBuilder().AtLevel(4).Build();

        var steps = member.GainExperience(50, _table);

        Assert.Empty(steps);
        Assert.Equal(4, member.Level);
        Assert.Equal(0, member.Experience);
    }

    [Fact]
    public void GainExperience_Zero_ReturnsNoStepsAndKeepsState()
    {
        var member = new PartyMemberBuilder().AtLevel(1, experience: 20).Build();

        var steps = member.GainExperience(0, _table);

        Assert.Empty(steps);
        Assert.Equal(1, member.Level);
        Assert.Equal(20, member.Experience);
    }

    [Fact]
    public void GainExperience_NegativeAmount_Throws()
    {
        var member = new PartyMemberBuilder().Build();

        Assert.Throws<ArgumentOutOfRangeException>(() => member.GainExperience(-1, _table));
    }
}
