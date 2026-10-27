using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Tests.Builders;

internal sealed class PartyMemberBuilder
{
    private string _id = "test_member";
    private string _classId = "fighter";
    private int _level = 1;
    private int _experience;
    private int _hp = 100;
    private int _maxHp = 100;
    private int? _weaponDamage;
    private SkillDefinition[] _skills = [];

    public PartyMemberBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public PartyMemberBuilder WithClass(string classId)
    {
        _classId = classId;
        return this;
    }

    public PartyMemberBuilder AtLevel(int level, int experience = 0)
    {
        _level = level;
        _experience = experience;
        return this;
    }

    public PartyMemberBuilder WithVitals(int hp, int maxHp)
    {
        _hp = hp;
        _maxHp = maxHp;
        return this;
    }

    /// <summary>
    /// A test weapon worn that deals <paramref name="attack"/>: a class has no attack (decision 330). Without it, the
    /// member hits for the neutral rules' unarmed 10.
    /// </summary>
    public PartyMemberBuilder WithAttack(int attack)
    {
        _weaponDamage = attack;
        return this;
    }

    public PartyMemberBuilder WithSkills(params SkillDefinition[] skills)
    {
        _skills = skills;
        return this;
    }

    /// <summary>
    /// Recruits, then restores the given progress, so a test can state any level and vitals. The member's class has the
    /// given numbers, the given skills and no growth, so the numbers hold at any level; the stat rules are neutral.
    /// </summary>
    public PartyMember Build()
    {
        var characterClass = new ClassBuilder()
            .WithId(_classId)
            .WithBase(new ClassStats(_maxHp, Speed: 10))
            .WithSkills([.. _skills.Select(skill => skill.Id)])
            .Build();
        var progress = new PartyMemberProgress(_id, _level, _experience, _hp, Worn: new Dictionary<EquipmentSlot, string>(), LastingAmounts: new Dictionary<string, int>());
        var member = PartyMember.Recruit(BuildDefinition(), characterClass, _skills.ToDictionary(skill => skill.Id), TestStatRules.Neutral);
        if (_weaponDamage is { } damage)
        {
            member.Wear(EquipmentSlot.Weapon, new ItemDefinitionBuilder().WithId("test_weapon").AsWeapon(damage).Build());
        }

        member.RestoreProgress(progress);
        return member;
    }

    public PartyMemberDefinition BuildDefinition() => new()
    {
        Id = _id,
        Name = "Test Member",
        ClassId = _classId,
        Summary = "A test member.",
        PortraitFile = "portrait.png",
        BattleSpriteFile = "battle.png",
        AttackEffectFile = "slash.png",
    };
}
