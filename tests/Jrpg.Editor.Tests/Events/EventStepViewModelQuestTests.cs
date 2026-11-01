using Jrpg.Core.Events;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The three quest steps in the dialogs (ADR 0032): start and complete pick a quest, advance picks one of its stages.
public sealed class EventStepViewModelQuestTests
{
    [Fact]
    public void FromStep_StartQuestStep_ShowsStartQuestWithItsId()
    {
        var step = EventStepViewModel.From(new StartQuestStep("errand"));

        Assert.Equal("errand", step.QuestId);
        Assert.Equal("Start quest: errand", step.Label);
        Assert.Null(step.Problem);
    }

    [Fact]
    public void FromStep_AdvanceQuestStep_SelectsItsStageChoice()
    {
        var step = EventStepViewModel.From(new AdvanceQuestStep("errand", "deliver"));

        Assert.Equal(new QuestStageChoice("errand", "deliver"), step.QuestStage);
        Assert.Equal("Advance quest: errand to deliver", step.Label);
    }

    [Fact]
    public void ToStep_AdvanceQuest_ReturnsItsQuestAndStage()
    {
        var step = EventStepViewModel.New(EventStepKind.AdvanceQuest);
        step.QuestStage = new QuestStageChoice("errand", "take");

        Assert.Null(step.Problem);
        Assert.Equal(new AdvanceQuestStep("errand", "take"), step.ToStep());
    }

    [Fact]
    public void ToStep_CompleteQuest_ReturnsCompleteQuestStep()
    {
        var step = EventStepViewModel.New(EventStepKind.CompleteQuest);
        step.QuestId = "errand";

        Assert.Equal("Complete quest: errand", step.Label);
        Assert.Equal(new CompleteQuestStep("errand"), step.ToStep());
    }

    [Fact]
    public void Problem_StartQuestWithoutQuest_AsksForTheQuest()
    {
        var step = EventStepViewModel.New(EventStepKind.StartQuest);

        Assert.Equal("Choose the quest.", step.Problem);
        Assert.Throws<InvalidOperationException>(step.ToStep);
    }

    [Fact]
    public void Problem_AdvanceQuestWithoutStage_AsksForTheStage()
    {
        var step = EventStepViewModel.New(EventStepKind.AdvanceQuest);

        Assert.Equal("Choose the quest's stage.", step.Problem);
        Assert.Throws<InvalidOperationException>(step.ToStep);
    }
}
