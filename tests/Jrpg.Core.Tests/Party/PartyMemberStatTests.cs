using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

// A member's stats: their class's at their level, and what worn gear adds or would add. With no weapon, a member hits
// for the neutral rules' unarmed 10; a weapon's damage replaces it (decision 330).
public sealed class PartyMemberStatTests
{
    private static readonly ClassDefinition _growingClass = new ClassBuilder()
        .WithBase(new ClassStats(MaxHp: 100, Speed: 0))
        .WithGrowth(new ClassStats(MaxHp: 10, Speed: 0))
        .Build();

    // 200 HP and 20 Constitution at level 1, +20 HP and +10 Constitution a level, at +1 % HP per Constitution point.
    private static readonly ClassDefinition _sturdyClass = new ClassBuilder()
        .WithBase(new ClassStats(MaxHp: 200, Speed: 0))
        .WithGrowth(new ClassStats(MaxHp: 20, Speed: 0))
        .WithPrimaries(new PrimaryStats(0, 0, 0, 20, 0), new PrimaryStats(0, 0, 0, 10, 0))
        .Build();

    private static readonly StatRules _rules = TestStatRules.Neutral with { HpPercentPer100Constitution = 100 };

    [Fact]
    public void RestoreProgress_LevelThree_MaxHpIsTheClassAtThree()
    {
        var progress = new PartyMemberProgress("test_member", 3, 0, Hp: 50, Worn: new Dictionary<EquipmentSlot, string>(), LastingAmounts: new Dictionary<string, int>());

        var member = PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(), _growingClass, new Dictionary<string, SkillDefinition>(), TestStatRules.Neutral);

        member.RestoreProgress(progress);

        Assert.Equal((50, 120), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void Sheet_WeaponWorn_AttackDamageIsTheWeapons()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").Build();
        Equip(member, new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(15).Build());

        Assert.Equal(15, member.Sheet.AttackDamage);
    }

    [Fact]
    public void SheetWithout_Weapon_FallsBackToUnarmed()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").Build();
        Equip(member, new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(15).Build());

        var preview = member.SheetWithout(EquipmentSlot.Weapon);

        Assert.Equal((10, 15), (preview.AttackDamage, member.Sheet.AttackDamage));
    }

    [Fact]
    public void SheetWith_OtherWeapon_UsesItsDamage()
    {
        var member = new PartyMemberBuilder().WithClass("fighter").Build();
        Equip(member, new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(15).Build());
        var ironSword = new ItemDefinitionBuilder().WithId("iron_sword").AsWeapon(18).Build();

        var preview = member.SheetWith(ironSword);

        Assert.Equal((18, 15), (preview.AttackDamage, member.Sheet.AttackDamage));
    }

    [Fact]
    public void Recruit_Constitution_RaisesMaxHp()
    {
        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), _sturdyClass, new Dictionary<string, SkillDefinition>(), _rules);

        Assert.Equal((240, 240), (member.Vitals.Hp, member.Vitals.MaxHp));
    }

    [Fact]
    public void GainExperience_LevelUp_MaxHpGrowsWithConstitution()
    {
        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), _sturdyClass, new Dictionary<string, SkillDefinition>(), _rules);

        member.GainExperience(100, new ExperienceTable([100, 100]));

        // Level 2: the class's 220 HP raised 30 % by 30 Constitution.
        Assert.Equal((286, 286), (member.Vitals.Hp, member.Vitals.MaxHp));
        Assert.Equal(30, member.Sheet.Primaries.Constitution);
    }

    [Fact]
    public void RestoreProgress_Constitution_KeepsTheSavedHpUnderTheRaisedMax()
    {
        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), _sturdyClass, new Dictionary<string, SkillDefinition>(), _rules);
        var progress = new PartyMemberProgress("test_member", 2, 0, Hp: 260, Worn: new Dictionary<EquipmentSlot, string>(), LastingAmounts: new Dictionary<string, int>());

        member.RestoreProgress(progress);

        Assert.Equal((260, 286), (member.Vitals.Hp, member.Vitals.MaxHp));
        Assert.Equal(30, member.Sheet.Primaries.Constitution);
    }

    [Fact]
    public void GainExperience_LevelUp_RaisesCritChanceWithAgility()
    {
        var nimbleClass = new ClassBuilder().WithPrimaries(new PrimaryStats(0, 20, 0, 0, 0), new PrimaryStats(0, 4, 0, 0, 0)).Build();
        var rules = TestStatRules.Neutral with { CritChancePer100Agility = 25 };
        var member = PartyMember.Recruit(new PartyMemberBuilder().BuildDefinition(), nimbleClass, new Dictionary<string, SkillDefinition>(), rules);
        Assert.Equal(50, member.Sheet.CritChancePermille);

        member.GainExperience(100, new ExperienceTable([100, 100]));

        // 24 Agility at +1 % per 4 points.
        Assert.Equal(60, member.Sheet.CritChancePermille);
    }

    private static void Equip(PartyMember member, ItemDefinition item)
    {
        var inventory = new Inventory();
        inventory.Add(item, 1);
        Assert.True(new PartyEquipment(inventory).TryEquip(member, item));
    }
}
