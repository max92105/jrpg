using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// Writing the catalogs and some maps together, all or none: what the editor's sheet purge saves (decision 442); and
/// some maps alone, checked together: what its Save and Save all write (decision 518).
/// </summary>
public sealed class ContentWriterMapsTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-maps", "test-world");
    private readonly ContentWriter _writer;

    public ContentWriterMapsTests() => _writer = new ContentWriter(_content.Path);

    public void Dispose() => _content.Dispose();

    // The forest's top-left cell erased and a tile blocked: a change to a map and a catalog in one write.
    [Fact]
    public void WriteMapCatalogsAndMaps_ValidChange_WritesCatalogsAndMaps()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var tile = catalog.Tiles.Values.First(tile => tile.IsWalkable) with { IsWalkable = false };
        var changed = WithForestTopLeft(catalog, TileLayer.EmptyTileId) with
        {
            Tiles = new Dictionary<int, TileDefinition>(catalog.Tiles) { [tile.Id] = tile },
        };

        _writer.WriteMapCatalogsAndMaps(changed, ["forest"]);

        var loaded = ContentCatalogLoader.Load(_content.Path);
        Assert.Equal(TileLayer.EmptyTileId, loaded.Maps["forest"].Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
        Assert.False(loaded.Tiles[tile.Id].IsWalkable);
    }

    [Fact]
    public void WriteMapCatalogsAndMaps_InvalidMap_WritesNothing()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var tile = catalog.Tiles.Values.First(tile => tile.IsWalkable) with { IsWalkable = false };
        var broken = WithForestTopLeft(catalog, catalog.Tiles.Keys.Max() + 1) with
        {
            Tiles = new Dictionary<int, TileDefinition>(catalog.Tiles) { [tile.Id] = tile },
        };
        var tilesBefore = File.ReadAllBytes(_content.PathOf("tiles.json"));
        var forestBefore = File.ReadAllBytes(Path.Combine(_content.Path, "map-layouts", "forest.json"));

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogsAndMaps(broken, ["forest"]));

        Assert.StartsWith("map-layouts/forest.json: ", exception.Message, StringComparison.Ordinal);
        Assert.Equal(tilesBefore, File.ReadAllBytes(_content.PathOf("tiles.json")));
        Assert.Equal(forestBefore, File.ReadAllBytes(Path.Combine(_content.Path, "map-layouts", "forest.json")));
    }

    // The deadlock of decision 518: the forest grown by three rows and start's warps moved into the new rows, start
    // refused against the saved forest, both valid together.
    [Fact]
    public void WriteMaps_TwoMapsValidOnlyTogether_WritesBoth()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var changed = WithStartWarpsIntoForestAt(WithForestGrownBy(catalog, 3), new GridCell(28, 19));

        _writer.WriteMaps(changed, ["forest", "start"]);

        var loaded = ContentCatalogLoader.Load(_content.Path);
        Assert.Equal(20, loaded.Maps["forest"].Layout.Height);
        Assert.Contains(loaded.Maps["start"].Layout.CellEvents, cellEvent => cellEvent.Event.Steps.Contains(new WarpStep("forest", new GridCell(28, 19))));
    }

    [Fact]
    public void WriteMaps_OneMapInvalid_WritesNothing()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var broken = WithStartWarpsIntoForestAt(catalog, new GridCell(28, 19));
        var startBefore = File.ReadAllBytes(LayoutPath("start"));
        var forestBefore = File.ReadAllBytes(LayoutPath("forest"));

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMaps(broken, ["forest", "start"]));

        Assert.StartsWith("map-layouts/start.json: ", exception.Message, StringComparison.Ordinal);
        Assert.Equal(startBefore, File.ReadAllBytes(LayoutPath("start")));
        Assert.Equal(forestBefore, File.ReadAllBytes(LayoutPath("forest")));
    }

    [Fact]
    public void WriteMaps_CatalogsChanged_LeavesTheCatalogFilesUntouched()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var tile = catalog.Tiles.Values.First(tile => tile.IsWalkable) with { IsWalkable = false };
        var changed = catalog with { Tiles = new Dictionary<int, TileDefinition>(catalog.Tiles) { [tile.Id] = tile } };
        var tilesBefore = File.ReadAllBytes(_content.PathOf("tiles.json"));

        _writer.WriteMaps(changed, ["forest"]);

        Assert.Equal(tilesBefore, File.ReadAllBytes(_content.PathOf("tiles.json")));
    }

    [Fact]
    public void CheckMaps_MapsValidOnlyTogether_DoesNotThrow()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var changed = WithStartWarpsIntoForestAt(WithForestGrownBy(catalog, 3), new GridCell(28, 19));

        ContentWriter.CheckMaps(changed);
    }

    [Fact]
    public void CheckMaps_WarpOutsideItsMap_ThrowsNamingTheLayout()
    {
        var broken = WithStartWarpsIntoForestAt(ContentCatalogLoader.Load(_content.Path), new GridCell(28, 19));

        var exception = Assert.Throws<ContentValidationException>(() => ContentWriter.CheckMaps(broken));

        Assert.StartsWith("map-layouts/start.json: ", exception.Message, StringComparison.Ordinal);
    }

    private string LayoutPath(string mapId) => Path.Combine(_content.Path, "map-layouts", $"{mapId}.json");

    private static ContentCatalog WithForestTopLeft(ContentCatalog catalog, int tileId)
    {
        var forest = catalog.Maps["forest"];
        var ground = forest.Layout.Layers[0];
        IReadOnlyList<int> row = [tileId, .. ground.Rows[0].Skip(1)];
        var layout = forest.Layout with { Layers = [ground with { Rows = [row, .. ground.Rows.Skip(1)] }, .. forest.Layout.Layers.Skip(1)] };
        return MapContent.With(catalog, new MapDefinition(layout, forest.Settings));
    }

    private static ContentCatalog WithForestGrownBy(ContentCatalog catalog, int rows)
    {
        var forest = catalog.Maps["forest"];
        var emptyRows = Enumerable.Range(0, rows).Select(_ => (IReadOnlyList<int>)new int[forest.Layout.Width]).ToList();
        var layout = forest.Layout with
        {
            Height = forest.Layout.Height + rows,
            Layers = [.. forest.Layout.Layers.Select(layer => layer with { Rows = [.. layer.Rows, .. emptyRows] })],
        };
        return MapContent.With(catalog, new MapDefinition(layout, forest.Settings));
    }

    private static ContentCatalog WithStartWarpsIntoForestAt(ContentCatalog catalog, GridCell cell)
    {
        var start = catalog.Maps["start"];
        var layout = start.Layout with
        {
            CellEvents = [.. start.Layout.CellEvents.Select(cellEvent => cellEvent with
            {
                Event = cellEvent.Event with
                {
                    Steps = [.. cellEvent.Event.Steps.Select(step => step is WarpStep { MapId: "forest" } ? new WarpStep("forest", cell) : step)],
                },
            })],
        };
        return MapContent.With(catalog, new MapDefinition(layout, start.Settings));
    }
}
