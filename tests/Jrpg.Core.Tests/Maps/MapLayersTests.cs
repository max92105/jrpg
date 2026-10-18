using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Maps;

public sealed class MapLayersTests
{
    [Fact]
    public void CanopyZOf_ObjectBelowPlayer_ReturnsAbovePlayerZ()
    {
        Assert.Equal(MapLayers.AbovePlayerZ, MapLayers.CanopyZOf(-3));
    }

    [Fact]
    public void CanopyZOf_ObjectAboveAbovePlayerZ_KeepsItsOwnZ()
    {
        Assert.Equal(7, MapLayers.CanopyZOf(7));
    }
}
