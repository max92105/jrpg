using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

/// <summary>The game's storage over Core's draw order (decision 520), whose rules Core's tests pin.</summary>
public sealed class MapDrawListTests
{
    [Fact]
    public void Rebuild_TwoMaps_ItemsFollowTheLastMap()
    {
        var drawList = new MapDrawList();
        drawList.Rebuild(Layout(layers: [-5, 5]), _ => throw new InvalidOperationException("No object."), new Point2D(0, 0));

        drawList.Rebuild(Layout(layers: [-5]), _ => throw new InvalidOperationException("No object."), new Point2D(0, 0));

        Assert.Equal([(MapDrawItemKind.Layer, 0), (MapDrawItemKind.Player, 0)], drawList.Items.Select(item => (item.Kind, item.Index)));
    }

    private static MapLayout Layout(int[] layers) => new()
    {
        Id = "test_map",
        Width = 10,
        Height = 20,
        Layers = [.. layers.Select(z => new TileLayer { Z = z, Rows = [] })],
        Objects = [],
    };
}
