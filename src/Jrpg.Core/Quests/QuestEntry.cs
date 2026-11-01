namespace Jrpg.Core.Quests;

/// <summary>One quest as the journal lists it: the quest and the stage it is at.</summary>
public sealed record QuestEntry(QuestDefinition Quest, QuestStage Stage);
