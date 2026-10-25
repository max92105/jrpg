using Jrpg.Core.Party;

namespace Jrpg.Core.Tests.Builders;

/// <summary>A class with given numbers. Without a growth its stats are the same at every level.</summary>
internal sealed class ClassBuilder
{
    private string _id = "fighter";
    private ClassStats _base = new(MaxHp: 100, MaxMp: 20, Attack: 10, Defense: 5);
    private ClassStats _growth = new(0, 0, 0, 0);
    private string[] _skillIds = [];

    public ClassBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ClassBuilder WithBase(ClassStats stats)
    {
        _base = stats;
        return this;
    }

    public ClassBuilder WithGrowth(ClassStats growth)
    {
        _growth = growth;
        return this;
    }

    public ClassBuilder WithSkills(params string[] ids)
    {
        _skillIds = ids;
        return this;
    }

    public ClassDefinition Build() => new(_id, _id, $"{_id} description", _base, _growth) { SkillIds = _skillIds };
}
