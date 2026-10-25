namespace Jrpg.Core.Party;

/// <summary>
/// A class, like a D&amp;D class (decision 164): what a character can do. The description shows when choosing a leader.
/// <see cref="Base"/> is the stats at level 1 and <see cref="Growth"/> what each later level adds (ADR 0017).
/// </summary>
public sealed record ClassDefinition(string Id, string Name, string Description, ClassStats Base, ClassStats Growth)
{
    /// <summary>Ids of the skills every member of the class has, in menu order (ADR 0020); empty when absent.</summary>
    public IReadOnlyList<string> SkillIds { get; init; } = [];
}
