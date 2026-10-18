using Jrpg.Core.World;

namespace Jrpg.Core.Tests.World;

public sealed class WorldStateTests
{
    [Fact]
    public void IsEventDone_SameEventIdOnOtherMap_ReturnsFalse()
    {
        var world = new WorldState();

        world.MarkEventDone(new MapEventKey("start", "chest_0"));

        Assert.True(world.IsEventDone(new MapEventKey("start", "chest_0")));
        Assert.False(world.IsEventDone(new MapEventKey("forest", "chest_0")));
    }

    [Fact]
    public void Constructor_SavedChanges_AreRestored()
    {
        var world = new WorldState([new MapEventKey("camp", "chest_0")], [new ObjectSwap("camp", "chest@1,0", "chest_opened")]);

        Assert.True(world.IsEventDone(new MapEventKey("camp", "chest_0")));
        Assert.Equal("chest_opened", world.SwappedObjectId("camp", "chest@1,0"));
    }

    [Fact]
    public void CompletedEventsAndObjectSwaps_AddedOutOfOrder_ComeOutInIdOrder()
    {
        var world = new WorldState();
        world.MarkEventDone(new MapEventKey("start", "door_0"));
        world.MarkEventDone(new MapEventKey("camp", "chest_1"));
        world.MarkEventDone(new MapEventKey("camp", "chest_0"));
        world.SwapObject(new ObjectSwap("start", "b", "door_open"));
        world.SwapObject(new ObjectSwap("camp", "z", "chest_opened"));
        world.SwapObject(new ObjectSwap("camp", "a", "chest_opened"));

        Assert.Equal(["camp/chest_0", "camp/chest_1", "start/door_0"], world.CompletedEvents.Select(key => $"{key.MapId}/{key.EventId}"));
        Assert.Equal(["camp/a", "camp/z", "start/b"], world.ObjectSwaps.Select(swap => $"{swap.MapId}/{swap.ObjectUid}"));
    }

    [Fact]
    public void SwappedObjectId_SwappedTwice_ReturnsTheLatest()
    {
        var world = new WorldState();

        world.SwapObject(new ObjectSwap("start", "door_1", "door_open"));
        world.SwapObject(new ObjectSwap("start", "door_1", "door_close"));

        Assert.Equal("door_close", world.SwappedObjectId("start", "door_1"));
        Assert.Null(world.SwappedObjectId("forest", "door_1"));
    }
}
