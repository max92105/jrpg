namespace Jrpg.Core.Resources;

/// <summary>A minimum of a resource the skill's user must hold, never spent: the cleric's faith (ADR 0019).</summary>
public sealed record ResourceRequirement(string ResourceId, int AtLeast);
