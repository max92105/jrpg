using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// Writing the catalogs and some maps together, all or none: what the editor's sheet purge saves (decision 442).
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

    private static ContentCatalog WithForestTopLeft(ContentCatalog catalog, int tileId)
    {
        var forest = catalog.Maps["forest"];
        var ground = forest.Layout.Layers[0];
        IReadOnlyList<int> row = [tileId, .. ground.Rows[0].Skip(1)];
        var layout = forest.Layout with { Layers = [ground with { Rows = [row, .. ground.Rows.Skip(1)] }, .. forest.Layout.Layers.Skip(1)] };
        return catalog with { Maps = new Dictionary<string, MapDefinition>(catalog.Maps) { ["forest"] = new MapDefinition(layout, forest.Settings) } };
    }
}
