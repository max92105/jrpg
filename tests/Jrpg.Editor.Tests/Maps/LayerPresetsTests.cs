using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

public sealed class LayerPresetsTests
{
    [Fact]
    public void NameOf_PresetZ_ReturnsItsName()
    {
        Assert.Equal("Ground", LayerPresets.NameOf(LayerPresets.GroundZ));
    }

    [Fact]
    public void NameOf_OtherZ_ReturnsLayer()
    {
        Assert.Equal("Layer", LayerPresets.NameOf(3));
    }
}
