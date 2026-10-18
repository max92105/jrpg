using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Events;

public sealed class MapEventTests
{
    [Fact]
    public void CanRunOn_OnlyWhenOtherObject_ReturnsFalse()
    {
        var openDoor = new MapEventBuilder().OnlyWhenObjectIs("door_close").Build();

        Assert.False(openDoor.CanRunOn("door_open"));
        Assert.True(openDoor.CanRunOn("door_close"));
    }

    [Fact]
    public void CanRunOn_NoCondition_RunsOnAnyObject()
    {
        var sign = new MapEventBuilder().Build();

        Assert.True(sign.CanRunOn("sign"));
    }
}
