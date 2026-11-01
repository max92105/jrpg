using Jrpg.Core.Quests;

namespace Jrpg.Game.Menus;

/// <summary>
/// One quest as the list draws it: the journal's entry and its title fitted to the column, once, when listed.
/// </summary>
internal sealed record QuestRow(QuestEntry Entry, string Title);
