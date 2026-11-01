namespace Jrpg.Core.Quests;

/// <summary>
/// The quest log as the player reads it (decisions 390, 391): the active quests, main before side, each newest first,
/// with the stage each is at; the completed ones, main before side, each oldest first, at the stage they were completed
/// at. Built when read, since the page reads it on opening, never per frame.
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
    public IReadOnlyList<QuestEntry> Active => Entries(_log.StartedQuests.Reverse(), isCompleted: false);

    /// <summary>The quests completed: main quests first, then side quests, each group oldest first.</summary>
    public IReadOnlyList<QuestEntry> Completed => Entries(_log.StartedQuests, isCompleted: true);

    // A stable sort by kind keeps the start order the quests come in within each kind.
    private IReadOnlyList<QuestEntry> Entries(IEnumerable<QuestDefinition> inOrder, bool isCompleted) =>
        [.. inOrder
            .Where(quest => _log.IsCompleted(quest.Id) == isCompleted)
            .OrderBy(quest => quest.Kind)
            .Select(quest => new QuestEntry(quest, quest.Stage(_log.StageOf(quest.Id)!)))];
}
