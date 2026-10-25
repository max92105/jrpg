namespace Jrpg.Core.Resources;

/// <summary>
/// This resource is a force rivalling <c>Over</c> (decision 195): while it dominates, the skills that feed it land
/// <c>BonusPercent</c> stronger. Dominance changes hands only when the other force goes strictly above (decision 204).
/// </summary>
public sealed record DominanceRule(string Over, int BonusPercent);
