using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

// A member of a class at a level wearing nothing (decision 948): the sheet's numbers, Constitution's share of max HP
// included, the same as a recruit's at that level.
public sealed class ClassSheetTests
{
    // 200 HP and 20 Constitution at level 1, +20 HP, +3 Power and +10 Constitution a level.
    private static readonly ClassDefinition _sturdyClass = new ClassBuilder()
        .WithBase(new ClassStats(MaxHp: 200, Speed: 7) { Accuracy = 4 })
        .WithGrowth(new ClassStats(MaxHp: 20, Speed: 1) { Accuracy = 2 })
        .WithPrimaries(new PrimaryStats(10, 6, 8, 20, 5), new PrimaryStats(3, 1, 2, 10, 4))
        .Build();

    // +1 % HP per Constitution point.
    private static readonly StatRules _rules = TestStatRules.Neutral with { HpPercentPer100Constitution = 100 };

    [Fact]
    public void At_LevelOne_IsTheBaseWithConstitutionsShare()
    {
        var sheet = ClassSheet.At(_sturdyClass, level: 1, _rules);

        Assert.Equal(240, sheet.MaxHp);
    }

    [Fact]
    public void At_LevelThree_AddsGrowthTwice()
    {
        var sheet = ClassSheet.At(_sturdyClass, level: 3, _rules);

        Assert.Equal(16, sheet.Primaries.Power);
    }

    [Fact]
    public void At_LevelZero_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ClassSheet.At(_sturdyClass, level: 0, _rules));

    [Fact]
    public void At_SameClassAndLevel_EqualsARecruitedMembersSheet()
    {
        var member = PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(), _sturdyClass, new Dictionary<string, SkillDefinition>(), _rules);
        member.GainExperience(30, new ExperienceTable([10, 20, 40]));

        var sheet = ClassSheet.At(_sturdyClass, level: 3, _rules);

        Assert.Equal((3, member.Sheet), (member.Level, sheet));
    }
}
