using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

public sealed class OpenMapsTests
{
    [Fact]
    public void Add_SameIdTwice_Throws()
    {
        var maps = new OpenMaps();
        maps.Add(MapNamed("meadow"));

        Assert.Throws<InvalidOperationException>(() => maps.Add(MapNamed("meadow")));
    }

    [Fact]
    public void Find_OpenMap_ReturnsTheSameViewModel()
    {
        var maps = new OpenMaps();
        var meadow = MapNamed("meadow");
        maps.Add(meadow);

        Assert.Same(meadow, maps.Find("meadow"));
        Assert.Null(maps.Find("forest"));
    }

    [Fact]
    public void Dirty_TwoMapsOneChanged_ListsIt()
    {
        var maps = new OpenMaps();
        var meadow = MapNamed("meadow");
        maps.Add(MapNamed("forest"));
        maps.Add(meadow);

        meadow.PutTiles(0, 3, [new GridCell(0, 0)]);

        Assert.Equal(["meadow"], maps.Dirty.Select(map => map.Map.Id));
    }

    [Fact]
    public void Dirty_TwoMapsChangedOutOfOrder_ListsThemByIdOrder()
    {
        var maps = new OpenMaps();
        var meadow = MapNamed("meadow");
        var forest = MapNamed("forest");
        maps.Add(meadow);
        maps.Add(forest);

        meadow.PutTiles(0, 3, [new GridCell(0, 0)]);
        forest.PutTiles(0, 3, [new GridCell(0, 0)]);

        Assert.Equal(["forest", "meadow"], maps.Dirty.Select(map => map.Map.Id));
    }

    [Fact]
    public void DirtyChanged_MapPainted_IsRaised()
    {
        var maps = new OpenMaps();
        var meadow = MapNamed("meadow");
        maps.Add(meadow);
        var raised = 0;
        maps.DirtyChanged += (_, _) => raised++;

        meadow.PutTiles(0, 3, [new GridCell(0, 0)]);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void ReplaceFrom_WrittenContent_RebuildsACleanMapAndDropsOneItLacks()
    {
        var written = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
        var maps = new OpenMaps();
        var forest = new MapViewModel(written.Maps["forest"]);
        maps.Add(forest);
        maps.Add(MapNamed("meadow"));

        maps.ReplaceFrom(written);

        Assert.Equal(["forest"], maps.All.Select(map => map.Map.Id));
        Assert.NotSame(forest, maps.Find("forest"));
    }

    [Fact]
    public void ReplaceFrom_MapWithUnsavedChanges_KeepsIt()
    {
        var written = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
        var maps = new OpenMaps();
        var meadow = MapNamed("meadow");
        maps.Add(meadow);
        meadow.PutTiles(0, 3, [new GridCell(0, 0)]);

        maps.ReplaceFrom(written);

        Assert.Same(meadow, maps.Find("meadow"));
    }

    [Fact]
    public void ReplaceFrom_ThenRebuiltMapPainted_RaisesDirtyChanged()
    {
        var written = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
        var maps = new OpenMaps();
        maps.Add(new MapViewModel(written.Maps["forest"]));
        maps.ReplaceFrom(written);
        var raised = 0;
        maps.DirtyChanged += (_, _) => raised++;

        maps.Find("forest")!.PutTiles(written.Maps["forest"].Layout.Layers[0].Z, 0, [new GridCell(0, 0)]);

        Assert.Equal(1, raised);
    }

    private static MapViewModel MapNamed(string id) => new(new MapDefinition(
        new MapLayout { Id = id, Width = 1, Height = 1, Layers = [new TileLayer { Z = 0, Rows = [[0]] }], Objects = [] },
        new MapSettings { Id = id, DisplayName = id }));
}
