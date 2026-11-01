namespace Jrpg.Core.Events;

/// <summary>
/// Completes a quest at its stage (ADR 0032); a quest not started is completed at its last stage, a completed one
/// stays.
/// </summary>
public sealed record CompleteQuestStep(string QuestId) : EventStep;
