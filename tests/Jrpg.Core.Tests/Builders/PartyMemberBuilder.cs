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
    private int _mp = 20;
    private int _maxMp = 20;
    private int _attack = 10;
    private int _defense = 5;
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

    public PartyMemberBuilder WithVitals(int hp, int maxHp, int mp, int maxMp)
    {
        _hp = hp;
        _maxHp = maxHp;
        _mp = mp;
        _maxMp = maxMp;
        return this;
    }

    public PartyMemberBuilder WithStats(int attack, int defense)
    {
        _attack = attack;
        _defense = defense;
        return this;
    }

    public PartyMemberBuilder WithSkills(params SkillDefinition[] skills)
    {
        _skills = skills;
        return this;
    }

    /// <summary>
    /// Restores rather than recruits, so a test can state any level and vitals. The member's class has the given
    /// numbers and no growth, so they hold at any level.
    /// </summary>
    public PartyMember Build()
    {
        var characterClass = new ClassBuilder()
            .WithId(_classId)
            .WithBase(new ClassStats(_maxHp, _maxMp, _attack, _defense))
            .Build();
        var progress = new PartyMemberProgress(_id, _level, _experience, _hp, _mp, WeaponId: null, ArmorId: null);
        return PartyMember.Restore(BuildDefinition(), characterClass, _skills.ToDictionary(skill => skill.Id), progress);
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
        SkillIds = [.. _skills.Select(skill => skill.Id)],
    };
}
