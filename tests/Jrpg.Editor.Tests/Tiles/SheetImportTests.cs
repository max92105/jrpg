using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class SheetImportTests : IDisposable
{
    // The repository's catalog, written into an empty content folder: only the tilesheet art is touched.
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"jrpg-import-{Guid.NewGuid():N}");
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")));
    private readonly SheetImport _import;
    private readonly List<string> _questions = [];

    public SheetImportTests()
    {
        _import = new SheetImport(new ContentWriter(_root), _draft);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    [Fact]
    public void Import_NewImage_ReturnsItsName()
    {
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "Cave.png"), 96, 48);

        var result = _import.Import([source], Ask(answer: true));

        Assert.Equal("Cave.png", result.LastImported);
        Assert.Empty(result.Refusals);
        Assert.Empty(_questions);
    }

    [Fact]
    public void Import_TakenNameDeclined_KeepsTheOldFileAndAsksNothingElse()
    {
        var old = PngHeaderFile.Write(TilesheetArt("Cave.png"), 96, 48);
        var oldBytes = File.ReadAllBytes(old);
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", "Cave.png"), 48, 48);

        var result = _import.Import([source], Ask(answer: false));

        Assert.Equal(["Cave.png"], _questions);
        Assert.Null(result.LastImported);
        Assert.Empty(result.Refusals);
        Assert.Equal(oldBytes, File.ReadAllBytes(old));
    }

    // A sheet of the catalog drawn from this file has tiles past a 1 x 1 image.
    [Fact]
    public void Import_RefusedReplacement_ReportsTheMessage()
    {
        var sheetFile = _draft.Current.Tilesheets[_draft.Current.Tiles.Values.First().SheetId].File;
        PngHeaderFile.Write(TilesheetArt(sheetFile), 1, 1);
        var source = PngHeaderFile.Write(Path.Combine(_root, "source", sheetFile), 1, 1);

        var result = _import.Import([source], Ask(answer: true));

        Assert.Null(result.LastImported);
        Assert.StartsWith($"{sheetFile}: tile ", Assert.Single(result.Refusals), StringComparison.Ordinal);
    }

    private string TilesheetArt(string fileName) => Path.Combine(ArtFolders.RootIn(_root), ArtFolders.Tilesheets, fileName);

    private Func<string, bool> Ask(bool answer) => fileName =>
    {
        _questions.Add(fileName);
        return answer;
    };
}
