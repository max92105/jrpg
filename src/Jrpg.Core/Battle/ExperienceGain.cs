using Jrpg.Core.Party;
using Jrpg.Core.Progression;

namespace Jrpg.Core.Battle;

/// <summary>
/// What one member got from a battle: the experience share, the level steps it produced, and whether they are at the
/// experience table's highest level after it.
/// </summary>
public sealed record ExperienceGain(PartyMember Member, int Amount, IReadOnlyList<LevelProgressStep> Steps, bool IsAtMaxLevel);
