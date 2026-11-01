namespace Jrpg.Core.Quests;

/// <summary>
/// One stage of a quest as content describes it: its id, and the text the page shows while the quest is at it.
/// </summary>
public sealed record QuestStage(string Id, string Text);
