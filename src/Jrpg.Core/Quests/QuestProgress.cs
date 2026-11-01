namespace Jrpg.Core.Quests;

/// <summary>
/// One quest's progress as a save keeps it (ADR 0032, format 7): its stage id and whether it is completed.
/// </summary>
public sealed record QuestProgress(string QuestId, string StageId, bool IsCompleted);
