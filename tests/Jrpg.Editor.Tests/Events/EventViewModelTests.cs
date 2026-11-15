using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// Which dialog an event is edited in (decision 728): an object's events show the prompt, the object condition and Swap
// object; a cell's event doesn't (CellEventViewModelTests has that side). A door with one event, built here.
public sealed class EventViewModelTests
{
    private static readonly PlacedObject _door = new()
    {
        Uid = "door_1",
        ObjectId = "door_close",
        Position = new Point2D(4, 4),
        Z = 1,
        Events =
        [
            new MapEvent
            {
                Id = "door_1_0",
                Activation = EventActivation.Interact,
                Prompt = "Open",
                Steps = [new SwapObjectStep("door_open")],
            },
        ],
    };

    private static readonly StepChoices _choices =
        new(["potion"], ["door_close", "door_open"], ["interior_1", "start"], ["omer"]);

    [Fact]
    public void IsObjectEvent_EventsOfTheObjectDialog_AreTrue()
    {
        var list = new EventListViewModel(_door, _choices, "door_open");

        list.AddEventCommand.Execute(null);

        Assert.Equal([true, true], list.Events.Select(mapEvent => mapEvent.IsObjectEvent));
    }

    [Fact]
    public void IsObjectEvent_PresetEvents_AreTrue()
    {
        var list = new EventListViewModel(_door, _choices, "door_open");

        list.ApplyDoorYouOpenPresetCommand.Execute(null);

        Assert.Equal([true, true], list.Events.Select(mapEvent => mapEvent.IsObjectEvent));
    }
}
