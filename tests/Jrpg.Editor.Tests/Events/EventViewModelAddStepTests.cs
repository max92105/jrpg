using Jrpg.Core.Events;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The Add step drop-down (decision 729): every step kind once, under its group's title; an entry adds its kind after
// the chosen step. On an object's event with one step, built here.
public sealed class EventViewModelAddStepTests
{
    private static readonly StepChoices _choices = new(["potion"], ["door_close", "door_open"], ["start"], ["omer"]);

    private readonly EventViewModel _event = new(
        new MapEvent { Id = "inn_1_0", Activation = EventActivation.Interact, Steps = [new RestStep()] },
        _choices);

    [Fact]
    public void AddStepChoices_AnyEvent_ListsTheKindsUnderFiveTitlesInOrder()
    {
        (string, bool)[] expected =
        [
            ("TALK", true), ("Dialogue", false), ("Voice", false), ("Cinematic", false),
            ("WORLD", true), ("Give item", false), ("Swap object", false), ("Warp", false), ("Move NPC", false),
            ("PARTY", true), ("Recruit", false), ("Rest", false),
            ("SCREENS", true), ("Open Forge", false), ("Open shop", false),
            ("QUESTS", true), ("Start quest", false), ("Advance quest", false), ("Complete quest", false),
        ];

        Assert.Equal(expected, _event.AddStepChoices.Select(choice => (choice.Label, choice.IsTitle)));
    }

    // A kind added to the enum without a menu entry fails here.
    [Fact]
    public void AddStepChoices_EveryKind_HasOneEntry()
    {
        var kinds = _event.AddStepChoices.Where(choice => !choice.IsTitle).Select(choice => (EventStepKind)choice.Parameter!);

        Assert.Equal(Enum.GetValues<EventStepKind>().Order(), kinds.Order());
    }

    [Fact]
    public void AddStepChoices_EachEntry_AddsItsKind()
    {
        foreach (var choice in _event.AddStepChoices.Where(choice => !choice.IsTitle))
        {
            choice.Command!.Execute(choice.Parameter);

            Assert.Equal((EventStepKind)choice.Parameter!, _event.SelectedStep!.Kind);
        }
    }

    [Fact]
    public void AddStepCommand_SwapObjectOnObjectEvent_CanExecute()
    {
        Assert.True(_event.AddStepCommand.CanExecute(EventStepKind.SwapObject));
    }
}
