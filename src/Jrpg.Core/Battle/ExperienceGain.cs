using Jrpg.Core.Party;
using Jrpg.Core.Progression;

namespace Jrpg.Core.Battle;

/// <summary>What one member got from a battle: the experience share and the level steps it produced.</summary>
public sealed record ExperienceGain(PartyMember Member, int Amount, IReadOnlyList<LevelProgressStep> Steps);
