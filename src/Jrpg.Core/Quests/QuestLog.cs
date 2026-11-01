namespace Jrpg.Core.Quests;

/// <summary>
/// Each started quest's stage and completion (ADR 0032), changed only by the three quest steps with decision 388's
/// rules for every state: a start on a started quest does nothing; an advance on a quest not started starts it at that
/// stage, may move back to an earlier stage, and leaves a completed quest alone; a complete on a quest not started
/// completes it at its last stage, and a completed quest stays completed. Stages are kept by id, so content inserting a
/// stage before the current one moves nothing.
/// </summary>
internal sealed class QuestLog
{
    private readonly Dictionary<string, Entry> _entries = [];

    /// <returns>The stage the quest is at, or was completed at; null while it was never started.</returns>
    public string? StageOf(string questId) => _entries.GetValueOrDefault(questId)?.StageId;

    public bool IsCompleted(string questId) => _entries.GetValueOrDefault(questId)?.IsCompleted ?? false;

    public void Start(QuestDefinition quest)
    {
        if (!_entries.ContainsKey(quest.Id))
        {
            _entries[quest.Id] = new Entry(quest.FirstStage.Id, IsCompleted: false);
        }
    }

    /// <exception cref="ArgumentException">
    /// The quest has no such stage (content is validated, so only a bug gets here).
    /// </exception>
    public void AdvanceTo(QuestDefinition quest, string stageId)
    {
        var stage = quest.Stage(stageId);
        if (IsCompleted(quest.Id))
        {
            return;
        }

        _entries[quest.Id] = new Entry(stage.Id, IsCompleted: false);
    }

    public void Complete(QuestDefinition quest)
    {
        if (IsCompleted(quest.Id))
        {
            return;
        }

        _entries[quest.Id] = new Entry(StageOf(quest.Id) ?? quest.LastStage.Id, IsCompleted: true);
    }

    private sealed record Entry(string StageId, bool IsCompleted);
}
