namespace Jrpg.Core.Battle;

/// <summary>The gold a defeated enemy drops, drawn once within both ends included (ADR 0031, decision 376).</summary>
/// <param name="Min">The least it drops, at least 0.</param>
/// <param name="Max">The most it drops, at least <paramref name="Min"/>.</param>
public sealed record GoldRange(int Min, int Max);
