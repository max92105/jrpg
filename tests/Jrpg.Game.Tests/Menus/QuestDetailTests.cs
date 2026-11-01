using Jrpg.Core.Quests;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

// The detail is the quest as the page says it (decision 392): the current stage's text, the kind's name.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class QuestDetailTests
{
    private static readonly QuestDefinition _errand = new(
        "errand", "An errand", "Carry a letter.", QuestKind.Side, [new QuestStage("take", "Take the letter."), new QuestStage("deliver", "Deliver it.")]);

    private static readonly UiMessages _messages = RepositoryScreenText.Text.Messages;

    [Fact]
    public void Of_ActiveQuest_HasItsCurrentStageText()
    {
        var detail = QuestDetail.Of(new QuestEntry(_errand, _errand.Stage("deliver")), QuestCategory.Active, _messages);

        Assert.Equal(("An errand", "Carry a letter.", "Deliver it."), (detail.Title, detail.Description, detail.StageText));
    }

    [Fact]
    public void Of_CompletedQuest_HasTheCompletedNoteNoStageText()
    {
        var detail = QuestDetail.Of(new QuestEntry(_errand, _errand.Stage("deliver")), QuestCategory.Completed, _messages);

        Assert.Equal(_messages.Text(MessageIds.QuestCompletedNote), detail.StageText);
        Assert.Equal(("An errand", "Carry a letter."), (detail.Title, detail.Description));
    }

    [Fact]
    public void Of_MainQuest_HasTheMainKindName()
    {
        var detail = QuestDetail.Of(new QuestEntry(_errand with { Kind = QuestKind.Main }, _errand.FirstStage), QuestCategory.Active, _messages);

        Assert.Equal(_messages.Text(MessageIds.QuestKindMain), detail.KindName);
    }

    [Fact]
    public void Of_SideQuest_HasTheSideKindName()
    {
        var detail = QuestDetail.Of(new QuestEntry(_errand, _errand.FirstStage), QuestCategory.Active, _messages);

        Assert.Equal(_messages.Text(MessageIds.QuestKindSide), detail.KindName);
        Assert.NotEqual(_messages.Text(MessageIds.QuestKindMain), detail.KindName);
    }
}
