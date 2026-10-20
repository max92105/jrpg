namespace Jrpg.Core.Effects;

/// <summary>Who one use of an effect can be aimed at, so a menu or battle UI only ever offers legal choices.</summary>
/// <typeparam name="T">A party member or an enemy.</typeparam>
/// <param name="Candidates">The legal targets, in party or formation order; empty when there are none.</param>
/// <param name="TakesAll">True when the effect hits every candidate at once, so there's nothing to choose.</param>
public sealed record TargetChoice<T>(IReadOnlyList<T> Candidates, bool TakesAll);
