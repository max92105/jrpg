namespace Jrpg.Core.Quests;

/// <summary>
/// What an event waits for of a quest before it may start (ADR 0033): the quest, one of the five kinds and, for the two
/// stage kinds, the stage. An NPC holds one event per quest situation, and the first that may start wins.
/// </summary>
/// <param name="QuestId">The quest asked about.</param>
/// <param name="Kind">What is asked of it.</param>
/// <param name="StageId">The stage the two stage kinds name; null for the others.</param>
public sealed record QuestCondition(string QuestId, QuestConditionKind Kind, string? StageId = null)
{
    internal bool IsMetBy(QuestLog log) => Kind switch
    {
        QuestConditionKind.NotStarted => log.StageOf(QuestId) is null,
        QuestConditionKind.Active => log.StageOf(QuestId) is not null && !log.IsCompleted(QuestId),
        QuestConditionKind.AtStage => !log.IsCompleted(QuestId) && log.StageOf(QuestId) == NamedStage(),
        QuestConditionKind.ReachedStage => log.HasReached(QuestId, NamedStage()),
        QuestConditionKind.Completed => log.IsCompleted(QuestId),
        _ => throw new InvalidOperationException($"Condition kind {Kind} on quest '{QuestId}' has no rule."),
    };

    // Content validation requires the stage on the two stage kinds, so a missing one is a bug.
    private string NamedStage() => StageId ?? throw new InvalidOperationException($"A {Kind} condition on quest '{QuestId}' names no stage.");
}
