namespace Jrpg.Core.Quests;

/// <summary>
/// What a quest step changed (decision 387): the quest and how; a step that changed nothing makes none.
/// </summary>
public sealed record QuestChange(QuestDefinition Quest, QuestChangeKind Kind);
