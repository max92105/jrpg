using Jrpg.Core.Quests;
using Jrpg.Editor.Quests;

namespace Jrpg.Editor.Tests.Quests;

// One quest as the Quests window edits it (decision 394): saved ids are fixed, a stage is added after the chosen one,
// only a new stage can be removed, and the problem names what the game would refuse, the stage by position.
public sealed class QuestViewModelTests
{
    // A two-stage quest of the tests' own: the repository's errand changes with content.
    private static readonly QuestDefinition _errand = new("placeholder_errand", "An errand", "Carry a question.", QuestKind.Side, [new QuestStage("ask_innkeeper", "Ask."), new QuestStage("return_to_giver", "Return.")]);

    [Fact]
    public void From_SavedQuest_IdsFixed()
    {
        var quest = QuestViewModel.From(_errand);

        Assert.False(quest.IsNew);
        Assert.Equal("placeholder_errand", quest.Id);
        Assert.Equal(_errand.Title, quest.Title);
        Assert.Equal(_errand.Kind, quest.Kind);
        Assert.All(quest.Stages, stage => Assert.False(stage.IsNew));
        Assert.Equal(_errand.Stages.Select(stage => stage.Id), quest.Stages.Select(stage => stage.Id));
    }

    [Fact]
    public void New_NothingTyped_IsNewWithAnEmptyId()
    {
        var quest = QuestViewModel.New();

        Assert.True(quest.IsNew);
        Assert.Equal("", quest.Id);
        Assert.Empty(quest.Stages);
        Assert.Equal("The id must not be empty.", quest.Problem);
    }

    [Fact]
    public void AddStage_StageChosen_AddsAnEditableStageAfterIt()
    {
        var quest = QuestViewModel.From(_errand);
        quest.SelectedStage = quest.Stages[0];

        quest.AddStageCommand.Execute(null);

        Assert.Equal(3, quest.Stages.Count);
        Assert.True(quest.Stages[1].IsNew);
        Assert.Same(quest.Stages[1], quest.SelectedStage);
        Assert.Equal("return_to_giver", quest.Stages[2].Id);
    }

    [Fact]
    public void RemoveStage_SavedStage_CannotExecute()
    {
        var quest = QuestViewModel.From(_errand);
        quest.SelectedStage = quest.Stages[0];

        Assert.False(quest.RemoveStageCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveStage_NewStage_RemovesItAndChoosesTheNext()
    {
        var quest = QuestViewModel.From(_errand);
        quest.SelectedStage = quest.Stages[0];
        quest.AddStageCommand.Execute(null);

        quest.RemoveStageCommand.Execute(null);

        Assert.Equal(["ask_innkeeper", "return_to_giver"], quest.Stages.Select(stage => stage.Id));
        Assert.Same(quest.Stages[1], quest.SelectedStage);
    }

    [Fact]
    public void MoveStageDown_FirstStage_BecomesSecond()
    {
        var quest = QuestViewModel.From(_errand);
        quest.SelectedStage = quest.Stages[0];

        quest.MoveStageDownCommand.Execute(null);

        Assert.Equal(["return_to_giver", "ask_innkeeper"], quest.Stages.Select(stage => stage.Id));
    }

    [Fact]
    public void Problem_NoStage_AsksForOne()
    {
        var quest = QuestViewModel.New();
        quest.Id = "heist";
        quest.Title = "The heist";
        quest.Description = "Case the vault.";

        Assert.Equal("Add at least one stage.", quest.Problem);
    }

    [Fact]
    public void Problem_RepeatedStageId_NamesTheLaterStage()
    {
        var quest = QuestViewModel.From(_errand);
        quest.SelectedStage = quest.Stages[0];
        quest.AddStageCommand.Execute(null);
        quest.SelectedStage!.Id = "ask_innkeeper";
        quest.SelectedStage.Text = "Ask again.";

        Assert.Equal("Stage 2 repeats the id ask_innkeeper.", quest.Problem);
    }

    [Fact]
    public void Problem_BlankStageText_NamesTheStageAndNotifies()
    {
        var quest = QuestViewModel.From(_errand);
        var notified = new List<string?>();
        quest.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        quest.Stages[1].Text = " ";

        Assert.Equal("Stage 2: Write the stage's text.", quest.Problem);
        Assert.Contains(nameof(QuestViewModel.Problem), notified);
    }

    [Fact]
    public void Problem_SavedQuestAsLoaded_IsNull()
    {
        Assert.Null(QuestViewModel.From(_errand).Problem);
    }

    [Fact]
    public void ToQuest_Edited_ReturnsTheDefinitionWithStagesInOrder()
    {
        var quest = QuestViewModel.From(_errand);
        quest.Title = "The errand";
        quest.SelectedStage = quest.Stages[0];
        quest.AddStageCommand.Execute(null);
        quest.SelectedStage!.Id = "wait";
        quest.SelectedStage.Text = "Wait a day. ";

        var definition = quest.ToQuest();

        Assert.Equal(
            new QuestDefinition("placeholder_errand", "The errand", _errand.Description, _errand.Kind, [_errand.Stages[0], new QuestStage("wait", "Wait a day."), _errand.Stages[1]]),
            definition);
    }

    [Fact]
    public void ToQuest_WithAProblem_Throws()
    {
        var quest = QuestViewModel.From(_errand);
        quest.Title = "";

        Assert.Throws<InvalidOperationException>(quest.ToQuest);
    }
}
