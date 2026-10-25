namespace Jrpg.Core.Resources;

/// <summary>What a skill does to one of its user's resources: a negative amount is a cost, a positive a gain.</summary>
public sealed record ResourceChange(string ResourceId, int Amount);
