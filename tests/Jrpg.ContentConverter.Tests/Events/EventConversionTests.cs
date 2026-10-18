using Jrpg.ContentConverter.Events;
using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;

namespace Jrpg.ContentConverter.Tests.Events;

public sealed class EventConversionTests
{
    private const string SourceFile = "Maps/start.json";

    private static readonly LegacyEventActionJson _swapToOpenDoor = new() { Type = LegacyEventActionType.SwapObject, ObjectId = 12 };
    private static readonly LegacyEventActionJson _warpInside = new()
    {
        Type = LegacyEventActionType.Warp,
        TargetMap = "Interior1.json",
        TargetX = 2,
        TargetY = 3,
    };

    private static readonly LegacyMapEventJson _openDoor = new()
    {
        Activation = LegacyEventActivation.Interact,
        Prompt = "Open",
        Once = false,
        OnlyWhenObjectId = 11,
        Actions = [_swapToOpenDoor],
    };

    private readonly ConversionReport _report = new();
    private readonly MapConversionContext _context;

    public EventConversionTests()
    {
        _context = new MapConversionContext(SourceFile, new Dictionary<int, string> { [11] = "door_close", [12] = "door_open" }, _report);
    }

    [Fact]
    public void ConvertTileTriggers_WarpTrigger_BecomesTouchCellEventWithWarpStep()
    {
        var trigger = new LegacyTriggerJson { Type = LegacyTriggerType.Warp, TargetMap = "Forest.json", TargetX = 1, TargetY = 1 };

        var cellEvent = Assert.Single(EventConversion.ConvertTileTriggers([(new GridCell(0, 14), trigger)], _context));

        Assert.Equal(new GridCell(0, 14), cellEvent.Cell);
        Assert.Equal(("cell_0_14", EventActivation.Touch), (cellEvent.Event.Id, cellEvent.Event.Activation));
        Assert.Equal([new WarpStep("forest", new GridCell(1, 1))], cellEvent.Event.Steps);
    }

    [Fact]
    public void ConvertTileTriggers_DialogueTrigger_KeepsSpeakerPortraitAndLines()
    {
        var trigger = new LegacyTriggerJson
        {
            Type = LegacyTriggerType.Dialogue,
            SpeakerName = "Old Man",
            PortraitPath = "Portraits/old_man.png",
            Lines = ["It's dangerous to go alone.", "Take this."],
        };

        var cellEvent = Assert.Single(EventConversion.ConvertTileTriggers([(new GridCell(3, 4), trigger)], _context));

        var dialogue = Assert.IsType<DialogueStep>(Assert.Single(cellEvent.Event.Steps));
        Assert.Equal(("Old Man", "Portraits/old_man.png"), (dialogue.Speech.Speaker, dialogue.Speech.PortraitFile));
        Assert.Equal(["It's dangerous to go alone.", "Take this."], dialogue.Speech.Lines);
    }

    [Fact]
    public void ConvertTileTriggers_BattleTrigger_IsSkippedAndReported()
    {
        var trigger = new LegacyTriggerJson { Type = LegacyTriggerType.Battle };

        var cellEvents = EventConversion.ConvertTileTriggers([(new GridCell(2, 2), trigger)], _context);

        Assert.Empty(cellEvents);
        Assert.Equal(["Maps/start.json: skipped the battle trigger on cell (2, 2) (event battles wait for question 30)."], _report.Lines);
    }

    [Fact]
    public void ConvertTileTriggers_ItemTrigger_ThrowsNamingTheCell()
    {
        var trigger = new LegacyTriggerJson { Type = LegacyTriggerType.Item, ItemId = "potion" };

        var exception = Assert.Throws<ConversionException>(() => EventConversion.ConvertTileTriggers([(new GridCell(2, 2), trigger)], _context));

        Assert.Equal("Maps/start.json: cell (2, 2) has a trigger of type Item, which the converter doesn't support.", exception.Message);
    }

    [Fact]
    public void ConvertTileTriggers_TwoTriggersOnOneCell_ThrowsNamingTheCell()
    {
        var trigger = new LegacyTriggerJson { Type = LegacyTriggerType.Warp, TargetMap = "Forest.json" };

        var exception = Assert.Throws<ConversionException>(
            () => EventConversion.ConvertTileTriggers([(new GridCell(0, 14), trigger), (new GridCell(0, 14), trigger)], _context));

        Assert.Equal("Maps/start.json: cell (0, 14) has two triggers.", exception.Message);
    }

    [Fact]
    public void ConvertObjectEvents_SecondEventOnObject_GetsIndexedId()
    {
        var enterDoor = _openDoor with { Activation = LegacyEventActivation.Touch, Prompt = "Enter", OnlyWhenObjectId = 12, Actions = [_warpInside] };

        var events = EventConversion.ConvertObjectEvents("e5ab86b9", [_openDoor, enterDoor], _context);

        Assert.Equal(["e5ab86b9_0", "e5ab86b9_1"], events.Select(mapEvent => mapEvent.Id));
        Assert.Equal([EventActivation.Interact, EventActivation.Touch], events.Select(mapEvent => mapEvent.Activation));
        Assert.Equal(["door_close", "door_open"], events.Select(mapEvent => mapEvent.OnlyWhenObjectId));
        Assert.Equal([new SwapObjectStep("door_open")], events[0].Steps);
        Assert.Equal([new WarpStep("interior_1", new GridCell(2, 3))], events[1].Steps);
    }

    [Fact]
    public void ConvertObjectEvents_BattleAction_IsSkippedAndReported()
    {
        var ambush = _openDoor with { Actions = [new LegacyEventActionJson { Type = LegacyEventActionType.Battle }, _swapToOpenDoor] };

        var mapEvent = Assert.Single(EventConversion.ConvertObjectEvents("e5ab86b9", [ambush], _context));

        Assert.Equal([new SwapObjectStep("door_open")], mapEvent.Steps);
        Assert.Equal(["Maps/start.json: skipped a battle action in event 'e5ab86b9_0' (event battles wait for question 30)."], _report.Lines);
    }

    [Fact]
    public void ConvertObjectEvents_OnlyBattleActions_SkipsTheEventButKeepsLaterIds()
    {
        var ambush = _openDoor with { Actions = [new LegacyEventActionJson { Type = LegacyEventActionType.Battle }] };

        var events = EventConversion.ConvertObjectEvents("e5ab86b9", [ambush, _openDoor], _context);

        Assert.Equal(["e5ab86b9_1"], events.Select(mapEvent => mapEvent.Id));
        Assert.Contains("Maps/start.json: skipped event 'e5ab86b9_0', which has no action left to run.", _report.Lines);
    }

    [Fact]
    public void ConvertObjectEvents_EmptyPrompt_BecomesCheck()
    {
        var mapEvent = Assert.Single(EventConversion.ConvertObjectEvents("e5ab86b9", [_openDoor with { Prompt = "" }], _context));

        Assert.Equal("Check", mapEvent.Prompt);
    }

    [Fact]
    public void ConvertObjectEvents_DialogueAndGiveItem_KeepTheirFields()
    {
        var chest = _openDoor with
        {
            Once = true,
            OnlyWhenObjectId = null,
            Actions =
            [
                new LegacyEventActionJson { Type = LegacyEventActionType.Dialogue, Speaker = "", Portrait = "Portraits/chest.png", Lines = ["A chest!"] },
                new LegacyEventActionJson { Type = LegacyEventActionType.GiveItem, ItemId = "potion", Quantity = 2 },
            ],
        };

        var mapEvent = Assert.Single(EventConversion.ConvertObjectEvents("0bff6243", [chest], _context));

        Assert.True(mapEvent.IsOnce);
        Assert.Null(mapEvent.OnlyWhenObjectId);
        Assert.Equal(new Speech(["A chest!"]).Lines, Assert.IsType<DialogueStep>(mapEvent.Steps[0]).Speech.Lines);
        Assert.Equal((null, "Portraits/chest.png"), (((DialogueStep)mapEvent.Steps[0]).Speech.Speaker, ((DialogueStep)mapEvent.Steps[0]).Speech.PortraitFile));
        Assert.Equal(new GiveItemStep("potion", 2), mapEvent.Steps[1]);
    }

    [Fact]
    public void ConvertObjectEvents_SwapToUnknownObject_ThrowsNamingTheEvent()
    {
        var swapToGhost = _openDoor with { Actions = [_swapToOpenDoor with { ObjectId = 99 }] };

        var exception = Assert.Throws<ConversionException>(() => EventConversion.ConvertObjectEvents("e5ab86b9", [swapToGhost], _context));

        Assert.Equal("Maps/start.json: event 'e5ab86b9_0' uses object id 99, which isn't in the object list.", exception.Message);
    }
}
