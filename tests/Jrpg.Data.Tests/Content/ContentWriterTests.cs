using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class ContentWriterTests : IDisposable
{
    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-writer-{Guid.NewGuid():N}");
    private readonly ContentCatalog _catalog;
    private readonly ContentWriter _writer;

    public ContentWriterTests()
    {
        CopyFolder(Path.Combine(AppContext.BaseDirectory, "content"), _contentRoot);
        _catalog = ContentCatalogLoader.Load(_contentRoot);
        _writer = new ContentWriter(_contentRoot);
    }

    public void Dispose() => Directory.Delete(_contentRoot, recursive: true);

    [Fact]
    public void WriteMap_ThenLoad_ReturnsEqualMap()
    {
        var start = _catalog.Maps["start"];
        var renamed = new MapDefinition(start.Layout, start.Settings with { DisplayName = "Starting village" });

        _writer.WriteMap(renamed, _catalog);
        var loaded = ContentCatalogLoader.Load(_contentRoot).Maps["start"];

        Assert.Equal(renamed.Settings, loaded.Settings);
        Assert.Equivalent(renamed.Layout, loaded.Layout, strict: true);
    }

    [Fact]
    public void WriteMap_InvalidWarpTarget_ThrowsAndLeavesFilesUntouched()
    {
        var start = _catalog.Maps["start"];
        var warpToNowhere = new CellEvent(
            new GridCell(0, 0),
            new MapEvent { Id = "warp_nowhere", Activation = EventActivation.Touch, Steps = [new WarpStep("nowhere", new GridCell(0, 0))] });
        var broken = new MapDefinition(start.Layout with { CellEvents = [.. start.Layout.CellEvents, warpToNowhere] }, start.Settings);
        var filesBefore = MapFileBytes("start");

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMap(broken, _catalog));

        Assert.StartsWith("map-layouts/start.json: ", exception.Message, StringComparison.Ordinal);
        Assert.Contains("'nowhere'", exception.Message, StringComparison.Ordinal);
        Assert.Equal(filesBefore, MapFileBytes("start"));
        Assert.Empty(Directory.EnumerateFiles(_contentRoot, "*.saving", SearchOption.AllDirectories));
    }

    [Fact]
    public void WriteMap_ShrinkingAMapAnotherMapWarpsInto_ThrowsNamingTheOtherMap()
    {
        // interior_1's door leads back into the start map, well inside its 45x45 cells.
        var start = _catalog.Maps["start"];
        var shrunk = new MapDefinition(ShrinkTo(start.Layout, 1, 1), start.Settings);

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMap(shrunk, _catalog));

        Assert.StartsWith("map-layouts/interior_1.json: ", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WriteMap_ShrinkingStartMapPastStartLocation_ThrowsNamingNewGame()
    {
        // 10 columns still hold every warp into the start map, but not the new game's start at (10, 3).
        var start = _catalog.Maps["start"];
        var narrowed = new MapDefinition(ShrinkTo(start.Layout, 10, start.Layout.Height) with { CellEvents = start.Layout.CellEvents }, start.Settings);
        var filesBefore = MapFileBytes("start");

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMap(narrowed, _catalog));

        Assert.StartsWith("new-game.json: ", exception.Message, StringComparison.Ordinal);
        Assert.Equal(filesBefore, MapFileBytes("start"));
    }

    [Theory]
    [InlineData("start")]
    [InlineData("forest")]
    [InlineData("interior_1")]
    public void WriteMap_UnchangedRepositoryMap_WritesIdenticalBytes(string mapId)
    {
        var filesBefore = MapFileBytes(mapId);

        _writer.WriteMap(_catalog.Maps[mapId], _catalog);

        Assert.Equal(filesBefore, MapFileBytes(mapId));
    }

    [Fact]
    public void WriteMap_NewMap_AddsBothFiles()
    {
        var forest = _catalog.Maps["forest"];
        var copy = new MapDefinition(forest.Layout with { Id = "forest_2" }, forest.Settings with { Id = "forest_2" });

        _writer.WriteMap(copy, _catalog);

        Assert.Equal("forest_2", ContentCatalogLoader.Load(_contentRoot).Maps["forest_2"].Settings.Id);
    }

    [Fact]
    public void WriteMapCatalogs_UnchangedRepositoryCatalogs_WritesIdenticalBytes()
    {
        var filesBefore = CatalogFileBytes();

        _writer.WriteMapCatalogs(_catalog);

        Assert.Equal(filesBefore, CatalogFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_NewTile_LoadsBackWithIt()
    {
        var tile = NewTile();

        _writer.WriteMapCatalogs(WithTile(tile));

        Assert.Equal(tile, ContentCatalogLoader.Load(_contentRoot).Tiles[tile.Id]);
    }

    [Fact]
    public void WriteMapCatalogs_TileOnUnknownSheet_ThrowsAndWritesNothing()
    {
        var filesBefore = CatalogFileBytes();

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(WithTile(NewTile() with { SheetId = "nowhere" })));

        Assert.StartsWith("tiles.json: ", exception.Message, StringComparison.Ordinal);
        Assert.Equal(filesBefore, CatalogFileBytes());
        Assert.Empty(Directory.EnumerateFiles(_contentRoot, "*.saving", SearchOption.AllDirectories));
    }

    [Fact]
    public void WriteMapCatalogs_TileOutsideItsImage_ThrowsNamingTheTileAndWritesNothing()
    {
        var tile = NewTile() with { Column = 9999 };
        var filesBefore = CatalogFileBytes();

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(WithTile(tile)));

        Assert.Contains($"tile {tile.Id}'s cell", exception.Message, StringComparison.Ordinal);
        Assert.Equal(filesBefore, CatalogFileBytes());
    }

    [Fact]
    public void WriteMapCatalogs_MapUsesMissingTile_ThrowsNamingTheMap()
    {
        var forest = _catalog.Maps["forest"];
        var ground = forest.Layout.Layers[0];
        var missingTileId = _catalog.Tiles.Keys.Max() + 1;
        var paintedRow = (IReadOnlyList<int>)[missingTileId, .. ground.Rows[0].Skip(1)];
        var layers = forest.Layout.Layers.Select(layer => layer == ground ? layer with { Rows = [paintedRow, .. layer.Rows.Skip(1)] } : layer);
        var painted = new MapDefinition(forest.Layout with { Layers = [.. layers] }, forest.Settings);
        var maps = _catalog.Maps.Values.Select(map => map.Id == "forest" ? painted : map).ToDictionary(map => map.Id);

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(_catalog with { Maps = maps }));

        Assert.StartsWith("map-layouts/forest.json: ", exception.Message, StringComparison.Ordinal);
    }

    // A tile on the first sheet that isn't in the catalog yet.
    private TileDefinition NewTile() => new()
    {
        Id = _catalog.Tiles.Keys.Max() + 1,
        IsWalkable = true,
        SheetId = _catalog.Tilesheets.Keys.Order(StringComparer.Ordinal).First(),
        Column = 0,
        Row = 0,
    };

    private ContentCatalog WithTile(TileDefinition tile) =>
        _catalog with { Tiles = _catalog.Tiles.Values.Append(tile).ToDictionary(other => other.Id) };

    private byte[] CatalogFileBytes() =>
        [.. File.ReadAllBytes(Path.Combine(_contentRoot, "tilesheets.json")),
         .. File.ReadAllBytes(Path.Combine(_contentRoot, "tiles.json")),
         .. File.ReadAllBytes(Path.Combine(_contentRoot, "objects.json"))];

    // Both files one after the other, compared byte for byte: line endings and encoding count.
    private byte[] MapFileBytes(string mapId) =>
        [.. File.ReadAllBytes(Path.Combine(_contentRoot, "map-layouts", $"{mapId}.json")),
         .. File.ReadAllBytes(Path.Combine(_contentRoot, "maps", $"{mapId}.json"))];

    private static MapLayout ShrinkTo(MapLayout layout, int width, int height) => layout with
    {
        Width = width,
        Height = height,
        Layers = [.. layout.Layers.Select(layer => layer with { Rows = [.. layer.Rows.Take(height).Select(row => (IReadOnlyList<int>)[.. row.Take(width)])] })],
        Objects = [],
        CellEvents = [],
    };

    private static void CopyFolder(string source, string destination)
    {
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(destination, Path.GetRelativePath(source, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }
    }
}
