using Jrpg.Core.Quests;
using Jrpg.Game.Dialogue;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Dialogue;

// Each change kind has its own line, naming the quest (decision 387).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class QuestNoticeTextTests
{
    private static readonly QuestDefinition _errand = new("errand", "An errand", "Carry a letter.", QuestKind.Side, [new QuestStage("take", "Take the letter.")]);
    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Theory]
    [InlineData(QuestChangeKind.Started, MessageIds.QuestStarted)]
    [InlineData(QuestChangeKind.Advanced, MessageIds.QuestUpdated)]
    [InlineData(QuestChangeKind.Completed, MessageIds.QuestCompleted)]
    public void Of_EachKind_UsesItsLineNamingTheQuest(QuestChangeKind kind, string messageId)
    {
        var line = QuestNoticeText.Of(new QuestChange(_errand, kind), _messages);

        Assert.Equal(_messages.Format(messageId, new MessageArgument("title", "An errand")), line);
        Assert.Contains("An errand", line, StringComparison.Ordinal);
    }

    [Fact]
    public void Of_TheThreeKinds_GiveThreeDifferentLines()
    {
        var lines = Enum.GetValues<QuestChangeKind>().Select(kind => QuestNoticeText.Of(new QuestChange(_errand, kind), _messages)).ToList();

        Assert.Equal(3, lines.Distinct(StringComparer.Ordinal).Count());
    }
}
