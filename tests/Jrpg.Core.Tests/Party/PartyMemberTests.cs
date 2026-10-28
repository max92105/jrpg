using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class PartyMemberTests
{
    private readonly ExperienceTable _table = new([100, 120, 144]);

    private static readonly ClassDefinition _growingClass = new ClassBuilder()
        .WithBase(new ClassStats(MaxHp: 100, Speed: 0))
        .WithGrowth(new ClassStats(MaxHp: 10, Speed: 0))
        .Build();

    [Fact]
    public void Recruit_Any_StartsAtLevelOneWithZeroExperienceAndFullVitals()
    {
        var definition = new PartyMemberBuilder().BuildDefinition();

        var member = PartyMember.Recruit(definition, _growingClass, new Dictionary<string, SkillDefinition>(), TestStatRules.Neutral);

        Assert.Equal((1, 0), (member.Level, member.Experience));
        Assert.Equal((100, 100), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void Recruit_ClassSkillIdNotInCatalog_Throws()
    {
        var definition = new PartyMemberBuilder().BuildDefinition();
        var healer = _growingClass with { SkillIds = ["heal"] };

        Assert.Throws<ArgumentException>(() => PartyMember.Recruit(definition, healer, new Dictionary<string, SkillDefinition>(), TestStatRules.Neutral));
    }

    [Fact]
    public void Recruit_ClassWithSkills_HasThemInClassOrder()
    {
        var healer = _growingClass with { SkillIds = ["recovery", "heal"] };

        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), healer, SkillCatalog(), TestStatRules.Neutral);

        Assert.Equal(["recovery", "heal"], member.Skills.Select(skill => skill.Id));
    }

    [Fact]
    public void Restore_ClassWithSkills_HasThem()
    {
        var healer = _growingClass with { SkillIds = ["heal"] };
        var progress = new PartyMemberProgress("test_member", 2, 0, Hp: 50, Worn: new Dictionary<EquipmentSlot, string>(), LastingAmounts: new Dictionary<string, int>());

        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), healer, SkillCatalog(), TestStatRules.Neutral);

        member.RestoreProgress(progress);

        Assert.Equal(["heal"], member.Skills.Select(skill => skill.Id));
    }

    [Fact]
    public void GainExperience_OneLevelUp_RaisesStatsByGrowth()
    {
        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), _growingClass, new Dictionary<string, SkillDefinition>(), TestStatRules.Neutral);
        member.Vitals.TakeDamage(40);

        member.GainExperience(100, _table);

        Assert.Equal((70, 110), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void GainExperience_TwoLevelUps_RaisesByTwoGrowths()
    {
        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), _growingClass, new Dictionary<string, SkillDefinition>(), TestStatRules.Neutral);

        member.GainExperience(220, _table);

        Assert.Equal(3, member.Level);
        Assert.Equal((120, 120), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void GainExperience_NoLevelUp_StatsUnchanged()
    {
        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), _growingClass, new Dictionary<string, SkillDefinition>(), TestStatRules.Neutral);

        member.GainExperience(99, _table);

        Assert.Equal((100, 100), (member.Vitals.Hp, member.Vitals.MaxHp));
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

    [Fact]
    public void EquippedIn_WornArmor_ReturnsIt()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").Build();
        var armor = new ItemDefinitionBuilder().WithId("chain_mail").AsChest().Build();
        Equip(member, armor);

        var worn = member.EquippedIn(EquipmentSlot.Chest)?.Base;

        Assert.Same(armor, worn);
    }

    [Fact]
    public void EquippedIn_EmptyWeaponSlot_ReturnsNull()
    {
        var member = new PartyMemberBuilder().Build();

        var worn = member.EquippedIn(EquipmentSlot.Weapon);

        Assert.Null(worn);
    }

    private static void Equip(PartyMember member, ItemDefinition item)
    {
        var inventory = new Inventory();
        inventory.Add(item, 1);
        Assert.True(new PartyEquipment(inventory).TryEquip(member, item));
    }

    private static Dictionary<string, SkillDefinition> SkillCatalog() =>
        new() { ["heal"] = Skill("heal"), ["recovery"] = Skill("recovery") };

    private static SkillDefinition Skill(string id) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.RestoreHp, 10, Targeting.SingleAlly),
    };
}
