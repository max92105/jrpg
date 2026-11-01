namespace Jrpg.Core.Quests;

/// <summary>
/// The quest log as the player reads it (decisions 390, 391): the active quests, main before side, each newest first,
/// with the stage each is at. Built when read, since the page reads it on opening, never per frame.
/// </summary>
public sealed class QuestJournal
{
    private readonly QuestLog _log;

    internal QuestJournal(QuestLog log)
    {
        _log = log;
    }

    /// <summary>
    /// The quests started and not completed: main quests first, then side quests, each group newest first.
    /// </summary>
    public IReadOnlyList<QuestEntry> Active =>
        [.. _log.StartedQuests
            .Reverse()
            .Where(quest => !_log.IsCompleted(quest.Id))
            .OrderBy(quest => quest.Kind)
            .Select(quest => new QuestEntry(quest, quest.Stage(_log.StageOf(quest.Id)!)))];
}
