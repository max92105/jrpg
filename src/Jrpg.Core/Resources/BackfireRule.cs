namespace Jrpg.Core.Resources;

/// <summary>A resource's danger (decision 194): each point is a chance that a spell turns on its caster.</summary>
/// <param name="ChancePerPointPercent">The chance per point held after the cast, in percent.</param>
/// <param name="DamagePercentOfMaxHp">What a backfire costs the caster, in percent of their max HP.</param>
public sealed record BackfireRule(int ChancePerPointPercent, int DamagePercentOfMaxHp);
