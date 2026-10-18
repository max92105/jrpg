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
    public void SwappedObjectId_SwappedTwice_ReturnsTheLatest()
    {
        var world = new WorldState();

        world.SwapObject(new ObjectSwap("start", "door_1", "door_open"));
        world.SwapObject(new ObjectSwap("start", "door_1", "door_close"));

        Assert.Equal("door_close", world.SwappedObjectId("start", "door_1"));
        Assert.Null(world.SwappedObjectId("forest", "door_1"));
    }
}
