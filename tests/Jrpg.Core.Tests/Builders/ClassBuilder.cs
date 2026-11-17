using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Stats;

namespace Jrpg.Core.Tests.Builders;

/// <summary>A class with given numbers. Without a growth its stats are the same at every level.</summary>
internal sealed class ClassBuilder
{
    private string _id = "fighter";
    private ClassStats _base = new(MaxHp: 100, Speed: 10);
    private ClassStats _growth = new(0, 0);
    private ClassSkill[] _skills = [];
    private ResourceDefinition[] _resources = [];
    private PrimaryStats? _basePrimaries;
    private PrimaryStats? _growthPrimaries;

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

    /// <summary>The primaries at level 1 and per level after, kept whatever <see cref="WithBase"/> sets.</summary>
    public ClassBuilder WithPrimaries(PrimaryStats basePrimaries, PrimaryStats growth)
    {
        _basePrimaries = basePrimaries;
        _growthPrimaries = growth;
        return this;
    }

    public ClassBuilder WithSkills(params string[] ids)
    {
        _skills = [.. ids.Select(id => new ClassSkill(id))];
        return this;
    }

    /// <summary>Skills each naming the level it is learned at (ADR 0062).</summary>
    public ClassBuilder WithSkills(params ClassSkill[] skills)
    {
        _skills = skills;
        return this;
    }

    public ClassBuilder WithResources(params ResourceDefinition[] resources)
    {
        _resources = resources;
        return this;
    }

    public ClassDefinition Build() => new(_id, _id, $"{_id} description", WithOwnPrimaries(_base, _basePrimaries), WithOwnPrimaries(_growth, _growthPrimaries))
    {
        Skills = _skills,
        Resources = _resources,
    };

    private static ClassStats WithOwnPrimaries(ClassStats stats, PrimaryStats? primaries) =>
        primaries is null ? stats : stats with { Primaries = primaries };
}
