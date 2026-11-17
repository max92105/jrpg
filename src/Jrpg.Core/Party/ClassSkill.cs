namespace Jrpg.Core.Party;

/// <summary>
/// One skill of a class with the level a member of it learns the skill at (ADR 0062); an entry naming no level is known
/// from level 1.
/// </summary>
public sealed record ClassSkill(string SkillId)
{
    public int Level { get; init; } = 1;
}
