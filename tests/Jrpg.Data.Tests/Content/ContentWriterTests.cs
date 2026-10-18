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
