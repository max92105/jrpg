namespace Jrpg.Editor.Events;

/// <summary>
/// One stage of one quest, as the advance step and the quest condition offer it: shown as <c>quest: stage</c>, equal by
/// value so an opened step selects its entry in the list.
/// </summary>
internal sealed record QuestStageChoice(string QuestId, string StageId)
{
    public string Label => $"{QuestId}: {StageId}";
}
