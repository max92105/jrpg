namespace Jrpg.Core.Stats;

/// <summary>
/// The stat conversions' rates and bases, from <c>rules.json</c> <c>stats</c> (ADR 0026). Every conversion is a percent
/// per point, written per 100 points so the numbers stay whole.
/// </summary>
/// <param name="HpPercentPer100Constitution">The percent of the class's max HP that 100 Constitution adds.</param>
public sealed record StatRules(int HpPercentPer100Constitution);
