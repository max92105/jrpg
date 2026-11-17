using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

/// <summary>
/// A member knows its class's skills at or below its level (ADR 0062). The test class lists a level-3 skill before the
/// level-1 one, so the class's order and the level order differ.
/// </summary>
public sealed class PartyMemberSkillLevelTests
{
    private readonly ExperienceTable _table = new([100, 120, 144]);

    private static readonly ClassDefinition _mage = new ClassBuilder()
        .WithId("mage")
        .WithSkills(new ClassSkill("meteor") { Level = 3 }, new ClassSkill("spark"), new ClassSkill("flame") { Level = 3 })
        .Build();

    [Fact]
    public void Recruit_LevelOne_KnowsOnlyLevelOneSkills()
    {
        var member = Recruit(_mage);

        Assert.Equal(["spark"], member.Skills.Select(skill => skill.Id));
    }

    [Fact]
    public void GainExperience_ReachingLevelThree_KnowsBothNewSkills()
    {
        var member = Recruit(_mage);

        member.GainExperience(220, _table);

        Assert.Equal(3, member.Level);
        Assert.Equal(["meteor", "spark", "flame"], member.Skills.Select(skill => skill.Id));
    }

    [Fact]
    public void GainExperience_ReachingLevelTwo_KnowsNoMore()
    {
        var member = Recruit(_mage);

        member.GainExperience(100, _table);

        Assert.Equal(2, member.Level);
        Assert.Equal(["spark"], member.Skills.Select(skill => skill.Id));
    }

    [Fact]
    public void RestoreProgress_LevelThree_KnowsEverySkillUpToIt()
    {
        var member = Recruit(_mage);

        member.RestoreProgress(ProgressAt(3));

        Assert.Equal(["meteor", "spark", "flame"], member.Skills.Select(skill => skill.Id));
    }

    [Fact]
    public void RestoreProgress_LevelTwo_KnowsOnlyLevelOne()
    {
        var member = Recruit(_mage);

        member.RestoreProgress(ProgressAt(2));

        Assert.Equal(["spark"], member.Skills.Select(skill => skill.Id));
    }

    [Fact]
    public void SkillsLearnedAt_LevelThree_NamesOnlyThatLevel()
    {
        var member = Recruit(_mage);

        var learned = member.SkillsLearnedAt(3);

        Assert.Equal(["meteor", "flame"], learned.Select(skill => skill.Id));
    }

    [Fact]
    public void SkillsLearnedAt_LevelTwo_NamesNone()
    {
        var member = Recruit(_mage);

        var learned = member.SkillsLearnedAt(2);

        Assert.Empty(learned);
    }

    // A skill not known yet is still checked at recruit: the catalog lacking it would fail at the level up otherwise.
    [Fact]
    public void Recruit_UnknownSkillIdAtALaterLevel_Throws()
    {
        var mage = _mage with { Skills = [new ClassSkill("spark"), new ClassSkill("unknown") { Level = 3 }] };

        Assert.Throws<ArgumentException>(() => Recruit(mage));
    }

    private static PartyMember Recruit(ClassDefinition characterClass) =>
        PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), characterClass, SkillCatalog(), TestStatRules.Neutral);

    private static PartyMemberProgress ProgressAt(int level) =>
        new("test_member", level, 0, Hp: 100, new Dictionary<EquipmentSlot, SavedGear>(), new Dictionary<string, int>());

    private static Dictionary<string, SkillDefinition> SkillCatalog() =>
        new() { ["spark"] = Skill("spark"), ["flame"] = Skill("flame"), ["meteor"] = Skill("meteor") };

    private static SkillDefinition Skill(string id) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy),
    };
}
