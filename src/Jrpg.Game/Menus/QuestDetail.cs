using Jrpg.Core.Quests;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// The chosen quest as the page shows it (decision 392): its title, its kind's name, its description and, for an active
/// quest, the text of the stage it is at; a completed quest shows the completed note in its place. Built when the
/// choice changes, never per frame.
/// </summary>
internal sealed record QuestDetail(string Title, string KindName, string Description, string StageText)
{
    public static QuestDetail Of(QuestEntry entry, QuestCategory category, UiMessages messages) =>
        new(
            entry.Quest.Title,
            messages.Text(KindIdOf(entry.Quest.Kind)),
            entry.Quest.Description,
            category == QuestCategory.Completed ? messages.Text(MessageIds.QuestCompletedNote) : entry.Stage.Text);

    private static string KindIdOf(QuestKind kind) => kind switch
    {
        QuestKind.Main => MessageIds.QuestKindMain,
        QuestKind.Side => MessageIds.QuestKindSide,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A quest kind with no line in ui.json."),
    };
}
