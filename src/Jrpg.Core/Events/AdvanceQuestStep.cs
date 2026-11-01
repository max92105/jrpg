namespace Jrpg.Core.Events;

/// <summary>
/// Moves a quest to a stage, an earlier one included (ADR 0032): a quest not started starts there; a completed one
/// stays as it is.
/// </summary>
public sealed record AdvanceQuestStep(string QuestId, string StageId) : EventStep;
