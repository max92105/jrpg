namespace Jrpg.Core.Events;

/// <summary>
/// Adds a party member as they join, at level 1 (question 179), then waits while the player reads who joined
/// (decision 163). A member already recruited stays as they are.
/// </summary>
public sealed record RecruitStep(string MemberId) : EventStep;
