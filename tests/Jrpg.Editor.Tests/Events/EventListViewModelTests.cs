using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

public sealed class EventListViewModelTests
{
    // A closed door: pressing it opens it, walking into the open door warps.
    private static readonly MapEvent _open = new()
    {
        Id = "door_1_0",
        Activation = EventActivation.Interact,
        Prompt = "Open",
        OnlyWhenObjectId = "door_close",
        Steps = [new SwapObjectStep("door_open")],
    };

    private static readonly MapEvent _enter = new()
    {
        Id = "door_1_1",
        Activation = EventActivation.Touch,
        OnlyWhenObjectId = "door_open",
        Steps = [new WarpStep("interior_1", new GridCell(2, 3))],
    };

    private static readonly PlacedObject _door = new()
    {
        Uid = "door_1",
        ObjectId = "door_close",
        Position = new Point2D(4, 4),
        Z = 1,
        Events = [_open, _enter],
    };

    private static readonly StepChoices _choices = new(["ether", "potion"], ["chest", "door_close", "door_open"], ["interior_1", "start"]);

    private readonly EventListViewModel _list = new(_door, _choices, "door_open");

    [Fact]
    public void Constructor_ObjectWithEvents_ListsThemAndSelectsTheFirst()
    {
        Assert.Equal(["Press \"Open\" if door_close: 1 step(s)", "Walk into if door_open: 1 step(s)"], _list.Events.Select(item => item.Label));
        Assert.Same(_list.Events[0], _list.SelectedEvent);
        Assert.Equal(["", "chest", "door_close", "door_open"], _list.OnlyWhenChoices);
    }

    [Fact]
    public void AddEventCommand_GivesTheNextIdAndTheDefaults()
    {
        _list.AddEventCommand.Execute(null);

        var added = _list.SelectedEvent!.ToEvent();
        Assert.Equal(("door_1_2", EventActivation.Interact, "Check"), (added.Id, added.Activation, added.Prompt));
    }

    [Fact]
    public void AddEventCommand_AfterRemovingTheLast_DoesNotReuseItsId()
    {
        _list.SelectedEvent = _list.Events[1];
        _list.RemoveEventCommand.Execute(null);

        _list.AddEventCommand.Execute(null);

        Assert.Equal("door_1_2", _list.SelectedEvent!.Id);
    }

    [Fact]
    public void RemoveEventCommand_SelectsTheEventThatTookItsPlace()
    {
        _list.RemoveEventCommand.Execute(null);

        Assert.Equal("door_1_1", _list.SelectedEvent?.Id);
    }

    [Fact]
    public void MoveEventDownCommand_ReorderedEvents_KeepTheirIds()
    {
        _list.MoveEventDownCommand.Execute(null);

        Assert.Equal(["door_1_1", "door_1_0"], _list.ToEdit()!.After.Select(mapEvent => mapEvent.Id));
    }

    [Fact]
    public void ToEdit_NothingChanged_IsNull()
    {
        Assert.Null(_list.ToEdit());
    }

    [Fact]
    public void ToEdit_UnchangedDialogue_IsNull()
    {
        var sign = _door with
        {
            Events = [_open with { Steps = [new DialogueStep(new Speech(["Welcome.", "Mind the step."], "Innkeeper", "innkeeper.png"))] }],
        };

        Assert.Null(new EventListViewModel(sign, _choices, "door_open").ToEdit());
    }

    [Fact]
    public void RemoveEventCommand_LastEvent_SelectsThePrevious()
    {
        _list.SelectedEvent = _list.Events[1];

        _list.RemoveEventCommand.Execute(null);

        Assert.Equal("door_1_0", _list.SelectedEvent?.Id);
    }

    [Fact]
    public void RemoveEventCommand_OnlyEvent_SelectsNothing()
    {
        _list.RemoveEventCommand.Execute(null);

        _list.RemoveEventCommand.Execute(null);

        Assert.Empty(_list.Events);
        Assert.Null(_list.SelectedEvent);
        Assert.False(_list.RemoveEventCommand.CanExecute(null));
    }

    [Fact]
    public void ToEdit_PromptChanged_ReplacesTheEvents()
    {
        _list.SelectedEvent!.Prompt = "Push";

        var edit = _list.ToEdit()!;

        Assert.Equal(("door_1", "Push"), (edit.Uid, edit.After[0].Prompt));
        Assert.Equal(_door.Events, edit.Before);
    }

    [Fact]
    public void AddDialogueCommand_StepSelected_InsertsAfterIt()
    {
        var open = _list.Events[0];
        open.SelectedStep = open.Steps[0];

        open.AddDialogueCommand.Execute(null);
        open.SelectedStep!.LinesText = "It creaks.";

        Assert.Equal(["Swap object to: door_open", "Dialogue: \"It creaks.\""], open.Steps.Select(step => step.Label));
    }

    [Fact]
    public void AddWarpCommand_AddsAWarpToCellZeroOfNoMapYet()
    {
        var open = _list.Events[0];

        open.AddWarpCommand.Execute(null);

        Assert.Equal("Warp to:  (0, 0)", open.SelectedStep!.Label);
        Assert.True(open.SelectedStep.IsWarp);
    }

    [Fact]
    public void Problem_QuantityLeftEmpty_NamesTheEventAndStep()
    {
        var open = _list.Events[0];
        open.AddGiveItemCommand.Execute(null);
        open.SelectedStep!.ItemId = "potion";

        open.SelectedStep!.QuantityText = "";

        Assert.Equal("Event door_1_0, step 2: The quantity must be a whole number of at least 1.", _list.Problem);
    }

    [Fact]
    public void ApplyChestPresetCommand_ReplacesTheEventsAndShowsTheItemToChoose()
    {
        _list.ApplyChestPresetCommand.Execute(null);

        var chest = Assert.Single(_list.Events);
        Assert.Equal("door_1_2", chest.Id);
        Assert.Same(chest, _list.SelectedEvent);
        Assert.Same(chest.Steps[0], chest.SelectedStep);
        Assert.Equal("Event door_1_2, step 1: Choose the item to give.", _list.Problem);
    }

    [Fact]
    public void ApplyDoorYouOpenPresetCommand_SelectsTheWarpToFill()
    {
        _list.ApplyDoorYouOpenPresetCommand.Execute(null);

        Assert.Same(_list.Events[1], _list.SelectedEvent);
        Assert.True(_list.SelectedEvent!.SelectedStep!.IsWarp);
    }

    [Fact]
    public void ApplySignPresetCommand_AsksForTheText()
    {
        _list.ApplySignPresetCommand.Execute(null);

        Assert.Equal("Event door_1_2, step 1: Write at least one line.", _list.Problem);
    }

    [Fact]
    public void MoveStepUpCommand_FirstStep_StaysFirst()
    {
        var open = _list.Events[0];
        open.SelectedStep = open.Steps[0];

        open.MoveStepUpCommand.Execute(null);

        Assert.Same(open.SelectedStep, open.Steps[0]);
    }
}
