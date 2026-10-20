namespace Jrpg.Core.Battle;

/// <summary>What a won battle gave, for the victory screen. Gold and drops join it later (developer decision 103).</summary>
/// <param name="Experience">The defeated enemies' total experience, before the split.</param>
/// <param name="Gains">One gain per living party member, in party order.</param>
public sealed record VictoryRewards(int Experience, IReadOnlyList<ExperienceGain> Gains);
