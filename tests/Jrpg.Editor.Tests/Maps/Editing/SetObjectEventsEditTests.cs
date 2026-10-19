using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class SetObjectEventsEditTests
{
    private static readonly IReadOnlyList<MapEvent> _open =
    [
        new MapEvent { Id = "door_1_0", Activation = EventActivation.Interact, Prompt = "Open", Steps = [new SwapObjectStep("door_open")] },
    ];

    private static readonly PlacedObject _door = new() { Uid = "door_1", ObjectId = "door_close", Position = new Point2D(4, 4), Z = 1, Events = _open };

    [Fact]
    public void Revert_RestoresPreviousEvents()
    {
        var draft = MapDraft.From(new MapDefinition(
            new MapLayout { Id = "hall", Width = 10, Height = 10, Layers = [], Objects = [_door] },
            new MapSettings { Id = "hall", DisplayName = "Hall" }));
        IReadOnlyList<MapEvent> greeting =
        [
            new MapEvent { Id = "door_1_1", Activation = EventActivation.Interact, Prompt = "Knock", Steps = [new DialogueStep(new Speech(["Hello?"]))] },
        ];
        var edit = new SetObjectEventsEdit("door_1", _open, greeting);
        edit.Apply(draft);
        Assert.Equal(greeting, draft.Objects[0].Events);

        edit.Revert(draft);

        Assert.Equal(_open, draft.Objects[0].Events);
    }
}
