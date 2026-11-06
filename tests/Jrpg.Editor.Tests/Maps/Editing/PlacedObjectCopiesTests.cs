using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

/// <summary>Copies of placed objects with fresh uids and event ids (decision 512), on records built in code.</summary>
public sealed class PlacedObjectCopiesTests
{
    private static readonly PlacedObject _chest1 = Placed("chest_1", "chest", 2.5, 3, EventOf("chest_1_0"), EventOf("chest_1_2"));
    private static readonly PlacedObject _chest3 = Placed("chest_3", "chest", 5, 1);

    [Fact]
    public void Make_OneObject_GetsTheNextUidOfItsObject()
    {
        var copies = PlacedObjectCopies.Make([_chest1], Target(_chest1, _chest3), new Point2D(0, 0));

        Assert.Equal("chest_4", Assert.Single(copies).Uid);
    }

    [Fact]
    public void Make_TwoCopiesOfOneObject_GetTwoDifferentUids()
    {
        var copies = PlacedObjectCopies.Make([_chest1, _chest3], Target(_chest1, _chest3), new Point2D(0, 0));

        Assert.Equal(["chest_4", "chest_5"], copies.Select(copy => copy.Uid));
    }

    [Fact]
    public void Make_EventsWithTheUidPattern_KeepTheirNumbersUnderTheNewUid()
    {
        var copies = PlacedObjectCopies.Make([_chest1], Target(_chest1, _chest3), new Point2D(0, 0));

        Assert.Equal(["chest_4_0", "chest_4_2"], copies[0].Events.Select(mapEvent => mapEvent.Id));
    }

    [Fact]
    public void Make_EventWithAnotherId_GetsTheNextFreeId()
    {
        var npc = Placed("npc_1", "npc", 1, 1, EventOf("talk"));

        var copies = PlacedObjectCopies.Make([npc], Target(npc), new Point2D(0, 0));

        Assert.Equal("npc_2_0", Assert.Single(copies[0].Events).Id);
    }

    // The target's cell event already holds chest_4_0, the id the copied chest_1_0 would keep.
    [Fact]
    public void Make_CellEventHoldingTheNewId_SkipsIt()
    {
        var chest = Placed("chest_1", "chest", 2.5, 3, EventOf("chest_1_0"));
        var target = Target(chest, _chest3) with { CellEvents = [new CellEvent(new GridCell(0, 0), EventOf("chest_4_0"))] };

        var copies = PlacedObjectCopies.Make([chest], target, new Point2D(0, 0));

        Assert.Equal("chest_4_1", Assert.Single(copies[0].Events).Id);
    }

    [Fact]
    public void Make_Group_KeepsTheOffsetsFromItsTopLeft()
    {
        var copies = PlacedObjectCopies.Make([_chest1, _chest3], Target(), new Point2D(10, 10));

        Assert.Equal([new Point2D(10, 12), new Point2D(12.5, 10)], copies.Select(copy => copy.Position));
    }

    [Fact]
    public void Make_KeepsObjectIdLayerAndEventContent()
    {
        var door = EventOf("door_1_0") with
        {
            Activation = EventActivation.Touch,
            Prompt = "Enter",
            IsOnce = true,
            OnlyWhenObjectId = "door",
            Steps = [new WarpStep("interior_1", new GridCell(4, 7)), new SwapObjectStep("door_open")],
        };
        var original = Placed("door_1", "door", 3, 3, door) with { Z = 4 };

        var copy = Assert.Single(PlacedObjectCopies.Make([original], Target(original), new Point2D(3, 3)));

        var copied = Assert.Single(copy.Events);
        Assert.Equal(("door", 4), (copy.ObjectId, copy.Z));
        Assert.Equal((EventActivation.Touch, "Enter", true, "door"), (copied.Activation, copied.Prompt, copied.IsOnce, copied.OnlyWhenObjectId));
        Assert.Equal(door.Steps, copied.Steps);
    }

    [Fact]
    public void Make_OntoTheMapItCameFrom_ReusesNoUidOrEventId()
    {
        var target = Target(_chest1, _chest3);

        var copies = PlacedObjectCopies.Make([_chest1, _chest3], target, new Point2D(0, 0));

        var targetIds = target.Objects.Select(placed => placed.Uid).Concat(target.AllEvents.Select(mapEvent => mapEvent.Id)).ToHashSet();
        Assert.DoesNotContain(copies.Select(copy => copy.Uid).Concat(copies.SelectMany(copy => copy.Events).Select(mapEvent => mapEvent.Id)), targetIds.Contains);
    }

    [Fact]
    public void TopLeftOf_Group_IsTheSmallestXAndY()
    {
        Assert.Equal(new Point2D(2.5, 1), PlacedObjectCopies.TopLeftOf([_chest1, _chest3]));
    }

    private static MapLayout Target(params PlacedObject[] objects) =>
        new() { Id = "glade", Width = 20, Height = 20, Layers = [], Objects = objects };

    private static PlacedObject Placed(string uid, string objectId, double x, double y, params MapEvent[] events) =>
        new() { Uid = uid, ObjectId = objectId, Position = new Point2D(x, y), Z = 1, Events = events };

    private static MapEvent EventOf(string id) => new() { Id = id, Activation = EventActivation.Interact, Steps = [] };
}
