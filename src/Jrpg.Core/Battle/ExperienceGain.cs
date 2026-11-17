using Jrpg.Core.Party;
using Jrpg.Core.Progression;

namespace Jrpg.Core.Battle;

/// <summary>
/// What one member got from a battle: the experience share, the level steps it produced, whether they are at the
/// experience table's highest level after it, and the skills the levels it reached taught.
/// </summary>
public sealed record ExperienceGain(PartyMember Member, int Amount, IReadOnlyList<LevelProgressStep> Steps, bool IsAtMaxLevel)
{
    /// <summary>
    /// By level reached, then in the class's order (ADR 0062); empty when no skill comes at those levels.
    /// </summary>
    public IReadOnlyList<LearnedSkill> LearnedSkills { get; init; } = [];
}
