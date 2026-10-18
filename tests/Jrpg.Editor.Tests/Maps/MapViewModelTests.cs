using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

public sealed class MapViewModelTests
{
    [Fact]
    public void Constructor_LayersOutOfZOrder_ExposesThemBottomFirst()
    {
        var start = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")).Maps["start"];
        var reversed = new MapDefinition(start.Layout with { Layers = [.. start.Layout.Layers.Reverse()] }, start.Settings);

        var viewModel = new MapViewModel(reversed);

        Assert.Equal([-5, -4], viewModel.Layers.Select(layer => layer.Z));
    }
}
