namespace Jrpg.Core.Battle;

/// <summary>
/// The experience a won battle gave, for the victory screen. The drops return beside it (<c>Session.BattleSpoils</c>,
/// ADR 0030); gold is still open (question 222).
/// </summary>
/// <param name="Experience">The defeated enemies' total experience, before the split.</param>
/// <param name="Gains">One gain per living party member, in party order.</param>
public sealed record VictoryRewards(int Experience, IReadOnlyList<ExperienceGain> Gains);
