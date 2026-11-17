using Jrpg.Core.Resources;

namespace Jrpg.Core.Party;

/// <summary>
/// A class, like a D&amp;D class (decision 164): what a character can do. The description shows when choosing a leader.
/// <see cref="Base"/> is the stats at level 1 and <see cref="Growth"/> what each later level adds (ADR 0017).
/// </summary>
public sealed record ClassDefinition(string Id, string Name, string Description, ClassStats Base, ClassStats Growth)
{
    /// <summary>
    /// The class's skills in menu order (ADR 0020), each with the level it is learned at (ADR 0062); empty when absent.
    /// </summary>
    public IReadOnlyList<ClassSkill> Skills { get; init; } = [];

    /// <summary>
    /// Every skill's id in menu order, whatever its level: for the readers of the class's whole list (decision 970).
    /// </summary>
    public IReadOnlyList<string> SkillIds => [.. Skills.Select(skill => skill.SkillId)];

    /// <summary>The battle resources every member of the class has (ADR 0019); empty when absent.</summary>
    public IReadOnlyList<ResourceDefinition> Resources { get; init; } = [];
}
