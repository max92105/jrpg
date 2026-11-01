using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Quests;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// An event's quest condition survives the dialog (QV's known gap closed) and shows in its one-line summary.
public sealed class EventQuestConditionTests
{
    private static readonly StepChoices _choices = new([], [], ["start"], ["sarxian"])
    {
        QuestIds = ["errand"],
        QuestStages = [new QuestStageChoice("errand", "take"), new QuestStageChoice("errand", "deliver")],
    };

    private static readonly MapEvent _talk = new()
    {
        Id = "villager_0",
        Activation = EventActivation.Interact,
        Prompt = "Talk",
        OnlyWhenQuest = new QuestCondition("errand", QuestConditionKind.ReachedStage, "deliver"),
        Steps = [new DialogueStep(new Speech(["Hello."])), new CompleteQuestStep("errand")],
    };

    [Fact]
    public void ToEvent_OpenedWithCondition_KeepsIt()
    {
        var talk = new EventViewModel(_talk, _choices);

        Assert.Equal(_talk.OnlyWhenQuest, talk.ToEvent().OnlyWhenQuest);
    }

    [Fact]
    public void Label_WithCondition_SaysWhen()
    {
        var talk = new EventViewModel(_talk, _choices);

        Assert.Equal("Press \"Talk\" when errand reached deliver: 2 step(s)", talk.Label);
    }

    [Fact]
    public void Problem_GateWithoutStage_ComesBeforeTheSteps()
    {
        var talk = new EventViewModel(_talk, _choices);
        talk.Steps[^1].QuestId = "";

        talk.QuestGate.StageId = "";

        Assert.Equal("Choose the stage the quest must have reached.", talk.Problem);
        Assert.Throws<InvalidOperationException>(talk.ToEvent);
    }
}
