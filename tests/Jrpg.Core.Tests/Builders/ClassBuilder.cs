using Jrpg.Core.Party;
using Jrpg.Core.Resources;

namespace Jrpg.Core.Tests.Builders;

/// <summary>A class with given numbers. Without a growth its stats are the same at every level.</summary>
internal sealed class ClassBuilder
{
    private string _id = "fighter";
    private ClassStats _base = new(MaxHp: 100, Attack: 10, Defense: 5, Speed: 10);
    private ClassStats _growth = new(0, 0, 0, 0);
    private string[] _skillIds = [];
    private ResourceDefinition[] _resources = [];

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

    public ClassBuilder WithResources(params ResourceDefinition[] resources)
    {
        _resources = resources;
        return this;
    }

    public ClassDefinition Build() => new(_id, _id, $"{_id} description", _base, _growth) { SkillIds = _skillIds, Resources = _resources };
}
