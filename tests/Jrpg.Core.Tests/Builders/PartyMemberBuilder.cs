using Jrpg.Core.Party;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Tests.Builders;

internal sealed class PartyMemberBuilder
{
    private string _id = "test_member";
    private string _classId = "warrior";
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

    public PartyMember Build() => PartyMember.Recruit(new PartyMemberDefinition
    {
        Id = _id,
        Name = "Test Member",
        ClassId = _classId,
        PortraitFile = "portrait.png",
        BattleSpriteFile = "battle.png",
        AttackEffectFile = "slash.png",
        Level = _level,
        Experience = _experience,
        Hp = _hp,
        MaxHp = _maxHp,
        Mp = _mp,
        MaxMp = _maxMp,
        Attack = _attack,
        Defense = _defense,
        SkillIds = [.. _skills.Select(skill => skill.Id)],
    }, _skills);
}
