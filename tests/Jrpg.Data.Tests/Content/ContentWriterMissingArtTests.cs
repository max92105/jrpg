using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Data.Tests.Art;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// Saving and importing with a sheet's image missing (decision 442): the content checks stay, the art check doesn't
/// block.
/// </summary>
public sealed class ContentWriterMissingArtTests : IDisposable
{
    private readonly CopiedContentRoot _content = new("writer-missing-art", "test-world");
    private readonly ContentWriter _writer;

    public ContentWriterMissingArtTests()
    {
        File.Delete(TilesheetImage("City.png"));
        _writer = new ContentWriter(_content.Path);
    }

    public void Dispose() => _content.Dispose();

    [Fact]
    public void WriteMapCatalogs_SheetImageMissing_WritesTheCatalogs()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var tile = catalog.Tiles.Values.First(tile => tile.SheetId == "city") with { IsWalkable = false };
        var changed = catalog with { Tiles = new Dictionary<int, TileDefinition>(catalog.Tiles) { [tile.Id] = tile } };

        _writer.WriteMapCatalogs(changed);

        Assert.Equal(tile, ContentCatalogLoader.Load(_content.Path).Tiles[tile.Id]);
    }

    [Fact]
    public void WriteMapCatalogs_TileOnUnknownSheet_StillThrows()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var stray = new TileDefinition { Id = catalog.Tiles.Keys.Max() + 1, IsWalkable = true, SheetId = "nowhere", Column = 0, Row = 0 };
        var broken = catalog with { Tiles = new Dictionary<int, TileDefinition>(catalog.Tiles) { [stray.Id] = stray } };

        var exception = Assert.Throws<ContentValidationException>(() => _writer.WriteMapCatalogs(broken));

        Assert.StartsWith("tiles.json: ", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ImportTilesheetImage_FileMissingFromTheFolder_CopiesIt()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var source = Path.Combine(AppContext.BaseDirectory, "test-world", "art", ArtFolders.Tilesheets, "City.png");

        _writer.ImportTilesheetImage(source, catalog);

        Assert.True(_writer.HasTilesheetImage("City.png"));
        Assert.DoesNotContain(ArtReferenceCheck.FindProblems(catalog, ArtFolders.RootIn(_content.Path)), problem => problem.Contains("City.png", StringComparison.Ordinal));
    }

    // The size check against the tiles cut from the sheet applies to the file coming back as to a replacement.
    [Fact]
    public void ImportTilesheetImage_FileMissingAndTooSmallForItsTiles_ThrowsNamingTheTile()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);
        var source = PngHeaderFile.Write(Path.Combine(_content.Path, "import-source", "City.png"), width: 48, height: 48);

        var exception = Assert.Throws<ContentValidationException>(() => _writer.ImportTilesheetImage(source, catalog));

        Assert.StartsWith("City.png: tile ", exception.Message, StringComparison.Ordinal);
        Assert.False(_writer.HasTilesheetImage("City.png"));
    }

    private string TilesheetImage(string file) => Path.Combine(ArtFolders.RootIn(_content.Path), ArtFolders.Tilesheets, file);
}
