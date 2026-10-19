using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class EventIdsTests
{
    [Fact]
    public void Next_NoEvents_StartsAtZero()
    {
        Assert.Equal("chest_1_0", EventIds.Next("chest_1", []));
    }

    [Fact]
    public void Next_AfterDeletingFirst_DoesNotReuseItsId()
    {
        Assert.Equal("e5ab86b9_2", EventIds.Next("e5ab86b9", ["e5ab86b9_1"]));
    }

    [Fact]
    public void Next_ReorderedEvents_GoesAboveTheHighest()
    {
        Assert.Equal("door_1_3", EventIds.Next("door_1", ["door_1_2", "door_1_0", "door_1_1"]));
    }

    [Fact]
    public void Next_IdsOfOtherObjectsOrNotNumbered_AreIgnored()
    {
        // "door_1_5" belongs to the object "door_1", not "door".
        Assert.Equal("door_0", EventIds.Next("door", ["door_1_5", "door_open"]));
    }
}
