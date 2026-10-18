using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

public sealed class MapViewModelTests
{
    // Two layers over a 2x1 map: ground everywhere, a detail tile on the right cell only.
    private static readonly MapViewModel _twoLayers = new(new MapDefinition(
        new MapLayout
        {
            Id = "meadow",
            Width = 2,
            Height = 1,
            Layers = [new TileLayer { Z = -5, Rows = [[1, 1]] }, new TileLayer { Z = -4, Rows = [[0, 7]] }],
            Objects = [],
        },
        new MapSettings { Id = "meadow", DisplayName = "Meadow" }));

    [Fact]
    public void Constructor_LayersOutOfZOrder_ExposesThemBottomFirst()
    {
        var start = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")).Maps["start"];
        var reversed = new MapDefinition(start.Layout with { Layers = [.. start.Layout.Layers.Reverse()] }, start.Settings);

        var viewModel = new MapViewModel(reversed);

        Assert.Equal([-5, -4], viewModel.Layers.Select(layer => layer.Z));
    }

    [Fact]
    public void TopTileAt_TilesOnTwoLayers_ReturnsTheHigherOne()
    {
        Assert.Equal(new TileAtCell(7, -4), _twoLayers.TopTileAt(new GridCell(1, 0)));
    }

    [Fact]
    public void TopTileAt_HigherLayerEmpty_ReturnsTheLowerOne()
    {
        Assert.Equal(new TileAtCell(1, -5), _twoLayers.TopTileAt(new GridCell(0, 0)));
    }

    [Fact]
    public void TopTileAt_OutsideTheMap_ReturnsNull()
    {
        Assert.Null(_twoLayers.TopTileAt(new GridCell(2, 0)));
    }
}
