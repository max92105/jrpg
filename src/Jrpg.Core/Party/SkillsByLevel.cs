using Jrpg.Core.Skills;

namespace Jrpg.Core.Party;

/// <summary>
/// A class's skills resolved from the skill catalog, each with the level a member of the class learns it at (ADR 0062):
/// what a member knows at its level and what reaching a level teaches it.
/// </summary>
internal sealed class SkillsByLevel
{
    private readonly List<(int Level, SkillDefinition Skill)> _entries;

    private SkillsByLevel(List<(int Level, SkillDefinition Skill)> entries) => _entries = entries;

    /// <exception cref="ArgumentException">A skill id isn't in the catalog.</exception>
    public static SkillsByLevel Resolve(ClassDefinition characterClass, IReadOnlyDictionary<string, SkillDefinition> skillCatalog)
    {
        var unknownSkillId = characterClass.SkillIds.FirstOrDefault(id => !skillCatalog.ContainsKey(id));
        if (unknownSkillId is not null)
        {
            throw new ArgumentException(
                $"Class '{characterClass.Id}' has unknown skill id '{unknownSkillId}'.", nameof(skillCatalog));
        }

        return new([.. characterClass.Skills.Select(skill => (skill.Level, skillCatalog[skill.SkillId]))]);
    }

    /// <summary>The skills learned at <paramref name="level"/> or below, in the class's order.</summary>
    public IReadOnlyList<SkillDefinition> KnownAt(int level) =>
        [.. _entries.Where(entry => entry.Level <= level).Select(entry => entry.Skill)];

    /// <summary>The skills learned at exactly <paramref name="level"/>, in the class's order.</summary>
    public IReadOnlyList<SkillDefinition> LearnedAt(int level) =>
        [.. _entries.Where(entry => entry.Level == level).Select(entry => entry.Skill)];
}
