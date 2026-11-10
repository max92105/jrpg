using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

public sealed class LayerChoiceViewModelTests
{
    private readonly LayerChoiceViewModel _layer = new();

    [Fact]
    public void TryAddLayer_WholeNumber_ChoosesThatZ()
    {
        var isAdded = _layer.TryAddLayer(" 2 ");

        Assert.True(isAdded);
        Assert.Equal(2, _layer.Z);
    }

    [Fact]
    public void TryAddLayer_PresetName_ChoosesItsZ()
    {
        var isAdded = _layer.TryAddLayer("above player (roofs, treetops)");

        Assert.True(isAdded);
        Assert.Equal(MapLayers.AbovePlayerZ, _layer.Z);
    }

    [Fact]
    public void Accepts_NotDimmed_TakesEveryLayer()
    {
        _layer.Z = 1;

        Assert.True(_layer.Accepts(-4));
        Assert.True(_layer.Accepts(1));
    }

    [Fact]
    public void Accepts_Dimmed_TakesOnlyTheChosen()
    {
        _layer.Z = 1;
        _layer.AreOtherLayersDimmed = true;

        Assert.True(_layer.Accepts(1));
        Assert.False(_layer.Accepts(-4));
    }

    [Fact]
    public void TryAddLayer_NotANumber_KeepsTheChoice()
    {
        var isAdded = _layer.TryAddLayer("flowers");

        Assert.False(isAdded);
        Assert.Equal(LayerPresets.GroundZ, _layer.Z);
    }
}
