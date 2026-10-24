using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Data.Tests.Art;

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
    public void WriteMap_ShrinkingIntroMapPastStartLocation_ThrowsNamingNewGame()
    {
        // 2 columns keep the intro events on them, but not the new game's start at (2.5, 2.5).
        var intro = _catalog.Maps["intro"];
        var kept = intro.Layout.CellEvents.Where(cellEvent => cellEvent.Cell.X < 2).ToList();
        var narrowed = new MapDefinition(ShrinkTo(intro.Layout, 2, intro.Layout.Height) with { CellEvents = kept }, intro.Settings);
        var filesBefore = MapFileBytes("intro");

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMap(narrowed, _catalog));

        Assert.StartsWith("new-game.json: ", exception.Message, StringComparison.Ordinal);
        Assert.Equal(filesBefore, MapFileBytes("intro"));
    }

    [Theory]
    [InlineData("start")]
    [InlineData("forest")]
    [InlineData("interior_1")]
    [InlineData("intro")]
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

    [Fact]
    public void ImportTilesheetImage_Png_CopiesIntoTilesheetArt()
    {
        var source = PngHeaderFile.Write(ImportSource("Forest2.png"), width: 96, height: 48);
        Assert.False(_writer.HasTilesheetImage("Forest2.png"));

        var imported = _writer.ImportTilesheetImage(source, _catalog);

        Assert.Equal("Forest2.png", imported);
        Assert.True(_writer.HasTilesheetImage("Forest2.png"));
        Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(TilesheetImage("Forest2.png")));
    }

    [Fact]
    public void ImportTilesheetImage_NotAPng_ThrowsAndCopiesNothing()
    {
        var source = Path.Combine(_contentRoot, "import-source", "Notes.png");
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        File.WriteAllText(source, "not an image");

        var exception = Assert.Throws<ContentValidationException>(() => _writer.ImportTilesheetImage(source, _catalog));

        Assert.Equal("Notes.png: not a PNG image.", exception.Message);
        Assert.False(_writer.HasTilesheetImage("Notes.png"));
    }

    // A 48 x 48 image holds only cell (0, 0) of a sheet cut into 48 px tiles; the lowest tile past it is named.
    [Fact]
    public void ImportTilesheetImage_ReplacementTooSmallForItsTiles_ThrowsNamingTheTileAndKeepsTheOldFile()
    {
        var (sheet, outside) = SheetWithATilePastItsFirstCell();
        var oldBytes = File.ReadAllBytes(TilesheetImage(sheet.File));
        var source = PngHeaderFile.Write(ImportSource(sheet.File), width: sheet.TileWidth, height: sheet.TileHeight);

        var exception = Assert.Throws<ContentValidationException>(() => _writer.ImportTilesheetImage(source, _catalog));

        Assert.Equal(
            $"{sheet.File}: tile {outside.Id} at ({outside.Column}, {outside.Row}) would be outside the new {sheet.TileWidth} x {sheet.TileHeight} image.",
            exception.Message);
        Assert.Equal(oldBytes, File.ReadAllBytes(TilesheetImage(sheet.File)));
    }

    // Windows finds "GROUND.PNG" as "Ground.png", so the smaller image would replace the sheet's file all the same.
    [Fact]
    public void ImportTilesheetImage_ReplacementNamedInOtherCase_StillChecksItsTiles()
    {
        var (sheet, outside) = SheetWithATilePastItsFirstCell();
        var source = PngHeaderFile.Write(ImportSource(sheet.File.ToUpperInvariant()), width: sheet.TileWidth, height: sheet.TileHeight);

        var exception = Assert.Throws<ContentValidationException>(() => _writer.ImportTilesheetImage(source, _catalog));

        Assert.Contains($"tile {outside.Id} ", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ImportTilesheetImage_ReplacementLargeEnough_ReplacesTheFile()
    {
        var (sheet, _) = SheetWithATilePastItsFirstCell();
        var (width, height) = PngSize.Read(TilesheetImage(sheet.File));
        var source = PngHeaderFile.Write(ImportSource(sheet.File), width, height);

        _writer.ImportTilesheetImage(source, _catalog);

        Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(TilesheetImage(sheet.File)));
    }

    // The first sheet, by id, with a tile cut from past its top-left cell; that tile is the lowest such one.
    private (TilesheetDefinition Sheet, TileDefinition Outside) SheetWithATilePastItsFirstCell() =>
        (from sheet in _catalog.Tilesheets.Values.OrderBy(sheet => sheet.Id, StringComparer.Ordinal)
         let outside = _catalog.Tiles.Values.Where(tile => tile.SheetId == sheet.Id && (tile.Column > 0 || tile.Row > 0)).MinBy(tile => tile.Id)
         where outside is not null && sheet.Margin == 0
         select (sheet, outside)).First();

    private string TilesheetImage(string fileName) => Path.Combine(ArtFolders.RootIn(_contentRoot), ArtFolders.Tilesheets, fileName);

    // Where a test puts an image before importing it: outside the art folder, as a file the developer picked would be.
    private string ImportSource(string fileName) => Path.Combine(_contentRoot, "import-source", fileName);

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
