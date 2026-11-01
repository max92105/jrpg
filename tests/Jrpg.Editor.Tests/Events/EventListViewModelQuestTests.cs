using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The quest step buttons add their step after the chosen one, or last; the added step keeps what is picked for it.
public sealed class EventListViewModelQuestTests
{
    private static readonly MapEvent _talk = new()
    {
        Id = "villager_0",
        Activation = EventActivation.Interact,
        Prompt = "Talk",
        Steps = [new DialogueStep(new Speech(["Hello."]))],
    };

    private static readonly PlacedObject _villager = new() { Uid = "villager", ObjectId = "villager", Position = new Point2D(4, 4), Z = 1, Events = [_talk] };

    private static readonly StepChoices _choices = new([], ["villager"], ["start"], ["sarxian"])
    {
        QuestIds = ["errand"],
        QuestStages = [new QuestStageChoice("errand", "take"), new QuestStageChoice("errand", "deliver")],
    };

    private readonly EventListViewModel _list = new(_villager, _choices, "villager");

    [Fact]
    public void QuestChoices_ListTheCatalogsQuestsAfterNone()
    {
        Assert.Equal(["", "errand"], _list.QuestChoices);
        Assert.Equal(
            [(QuestConditionKind.NotStarted, "not started"), (QuestConditionKind.Active, "active"), (QuestConditionKind.AtStage, "at stage"), (QuestConditionKind.ReachedStage, "reached stage"), (QuestConditionKind.Completed, "completed")],
            _list.ConditionKinds.Select(choice => (choice.Kind, choice.Label)));
    }

    // OK reads the list's problem: a gate without its stage is refused with the gate's words, naming the event.
    [Fact]
    public void Problem_GateWithoutStage_IsTheGatesProblemNamingTheEvent()
    {
        var talk = _list.SelectedEvent!;
        talk.QuestGate.QuestId = "errand";

        talk.QuestGate.Kind = QuestConditionKind.ReachedStage;

        Assert.Equal("Event villager_0, Choose the stage the quest must have reached.", _list.Problem);
        talk.QuestGate.StageId = "deliver";
        Assert.Null(_list.Problem);
    }

    [Fact]
    public void AddAdvanceQuestCommand_NoStepChosen_AddsItLast()
    {
        var talk = _list.SelectedEvent!;

        talk.AddAdvanceQuestCommand.Execute(null);

        var added = talk.Steps[^1];
        Assert.Equal(EventStepKind.AdvanceQuest, added.Kind);
        Assert.Same(added, talk.SelectedStep);
        added.QuestStage = _choices.QuestStages[1];
        Assert.Equal(new AdvanceQuestStep("errand", "deliver"), talk.ToEvent().Steps[^1]);
    }

    [Fact]
    public void AddStartAndCompleteQuestCommands_WithTheQuestPicked_RoundTrip()
    {
        var talk = _list.SelectedEvent!;

        talk.AddStartQuestCommand.Execute(null);
        talk.Steps[^1].QuestId = "errand";
        talk.AddCompleteQuestCommand.Execute(null);
        talk.Steps[^1].QuestId = "errand";

        Assert.Equal([new StartQuestStep("errand"), new CompleteQuestStep("errand")], talk.ToEvent().Steps.Skip(1));
    }
}
