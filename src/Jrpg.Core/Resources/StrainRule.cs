namespace Jrpg.Core.Resources;

/// <summary>
/// How a lasting resource's missing points lower another resource's start (ADR 0024, question 219): each point below
/// its max takes <c>PerMissingPoint</c> off <c>LowersStartOf</c>'s starting amount, never below 0.
/// </summary>
public sealed record StrainRule(string LowersStartOf, int PerMissingPoint);
