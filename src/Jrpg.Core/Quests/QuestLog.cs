namespace Jrpg.Core.Quests;

/// <summary>
/// Each started quest's stage and completion (ADR 0032), changed only by the three quest steps with decision 388's
/// rules for every state: a start on a started quest does nothing; an advance on a quest not started starts it at that
/// stage, may move back to an earlier stage, and leaves a completed quest alone; a complete on a quest not started
/// completes it at its last stage, and a completed quest stays completed. Stages are kept by id, so content inserting a
/// stage before the current one moves nothing; only <see cref="HasReached"/> reads positions, in the quest's current
/// order (ADR 0033).
/// </summary>
internal sealed class QuestLog
{
    private readonly Dictionary<string, Entry> _entries = [];

    /// <returns>The stage the quest is at, or was completed at; null while it was never started.</returns>
    public string? StageOf(string questId) => _entries.GetValueOrDefault(questId)?.StageId;

    public bool IsCompleted(string questId) => _entries.GetValueOrDefault(questId)?.IsCompleted ?? false;

    /// <summary>
    /// Whether the quest is at <paramref name="stageId"/> or past it in its stage order, or completed; false while it
    /// was never started. An advance back un-reaches the later stages (decision 389).
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The quest has no such stage (content is validated, so only a bug gets here).
    /// </exception>
    public bool HasReached(string questId, string stageId)
    {
        if (_entries.GetValueOrDefault(questId) is not { } entry)
        {
            return false;
        }

        var named = PositionOf(entry.Quest, stageId);
        return entry.IsCompleted || PositionOf(entry.Quest, entry.StageId) >= named;
    }

    public void Start(QuestDefinition quest)
    {
        if (!_entries.ContainsKey(quest.Id))
        {
            _entries[quest.Id] = new Entry(quest, quest.FirstStage.Id, IsCompleted: false);
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

        _entries[quest.Id] = new Entry(quest, stage.Id, IsCompleted: false);
    }

    public void Complete(QuestDefinition quest)
    {
        if (IsCompleted(quest.Id))
        {
            return;
        }

        _entries[quest.Id] = new Entry(quest, StageOf(quest.Id) ?? quest.LastStage.Id, IsCompleted: true);
    }

    private static int PositionOf(QuestDefinition quest, string stageId)
    {
        var stages = quest.Stages;
        for (var position = 0; position < stages.Count; position++)
        {
            if (stages[position].Id == stageId)
            {
                return position;
            }
        }

        throw new ArgumentException($"Quest '{quest.Id}' has no stage '{stageId}'.", nameof(stageId));
    }

    // The definition is kept so a condition reads the stage order without a catalog (ADR 0033).
    private sealed record Entry(QuestDefinition Quest, string StageId, bool IsCompleted);
}
