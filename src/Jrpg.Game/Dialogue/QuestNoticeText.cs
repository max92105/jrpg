using Jrpg.Core.Quests;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Dialogue;

/// <summary>
/// The line for a quest step that changed the log (decision 387): started, updated or completed, naming the quest.
/// </summary>
internal static class QuestNoticeText
{
    public static string Of(QuestChange change, UiMessages messages) =>
        messages.Format(IdOf(change.Kind), new MessageArgument("title", change.Quest.Title));

    private static string IdOf(QuestChangeKind kind) => kind switch
    {
        QuestChangeKind.Started => MessageIds.QuestStarted,
        QuestChangeKind.Advanced => MessageIds.QuestUpdated,
        QuestChangeKind.Completed => MessageIds.QuestCompleted,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A quest change with no line in ui.json."),
    };
}
