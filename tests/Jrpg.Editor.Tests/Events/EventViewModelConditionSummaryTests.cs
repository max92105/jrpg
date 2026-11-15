using Jrpg.Core.Events;
using Jrpg.Core.Quests;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The folded conditions' line (decision 732): the event list's words without the step count, then ", once". The words
// themselves are Label's, tested with it; these test the join and what raises it. Events built here.
public sealed class EventViewModelConditionSummaryTests
{
    private static readonly StepChoices _choices = new([], ["door_close", "door_open"], ["start"], ["omer"])
    {
        QuestIds = ["errand"],
        QuestStages = [new QuestStageChoice("errand", "take")],
    };

    private static readonly MapEvent _door = new()
    {
        Id = "door_1_0",
        Activation = EventActivation.Interact,
        Prompt = "Open",
        IsOnce = true,
        OnlyWhenObjectId = "door_close",
        Steps = [new SwapObjectStep("door_open")],
    };

    [Fact]
    public void ConditionSummary_Once_IsTheWordsThenOnce()
    {
        var door = new EventViewModel(_door, _choices);

        Assert.Equal("Press \"Open\" if door_close, once", door.ConditionSummary);
    }

    [Fact]
    public void ConditionSummary_NotOnce_IsTheWordsAlone()
    {
        var intro = new EventViewModel(
            new MapEvent
            {
                Id = "cell_3_4",
                Activation = EventActivation.Arrive,
                OnlyWhenLeaderId = "omer",
                OnlyWhenQuest = new QuestCondition("errand", QuestConditionKind.Active),
                Steps = [new RestStep()],
            },
            _choices);

        Assert.Equal("On arrival while omer leads when errand is active", intro.ConditionSummary);
    }

    [Fact]
    public void ConditionSummary_QuestGateChanged_IsRaised()
    {
        var door = new EventViewModel(_door, _choices);
        var raised = new List<string?>();
        door.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        door.QuestGate.QuestId = "errand";

        Assert.Contains(nameof(EventViewModel.ConditionSummary), raised);
    }

    [Theory]
    [InlineData(nameof(EventViewModel.Activation))]
    [InlineData(nameof(EventViewModel.Prompt))]
    [InlineData(nameof(EventViewModel.IsOnce))]
    [InlineData(nameof(EventViewModel.OnlyWhenObjectId))]
    [InlineData(nameof(EventViewModel.OnlyWhenLeaderId))]
    public void ConditionSummary_ConditionFieldChanged_IsRaised(string field)
    {
        var door = new EventViewModel(_door, _choices);
        var raised = new List<string?>();
        door.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        Change(door, field);

        Assert.Contains(nameof(EventViewModel.ConditionSummary), raised);
    }

    private static void Change(EventViewModel door, string field)
    {
        switch (field)
        {
            case nameof(EventViewModel.Activation):
                door.Activation = EventActivation.Touch;
                break;
            case nameof(EventViewModel.Prompt):
                door.Prompt = "Push";
                break;
            case nameof(EventViewModel.IsOnce):
                door.IsOnce = false;
                break;
            case nameof(EventViewModel.OnlyWhenObjectId):
                door.OnlyWhenObjectId = "door_open";
                break;
            default:
                door.OnlyWhenLeaderId = "omer";
                break;
        }
    }
}
