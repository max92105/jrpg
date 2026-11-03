using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

public sealed class ArtStatusTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Warning_ArtWhole_IsNull()
    {
        var status = StatusOf(_content.TestWorld);

        Assert.Null(status.Warning);
        Assert.Empty(status.Missing.SheetIds);
    }

    [Fact]
    public void Warning_SheetImageDeleted_NamesTheFile()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(TilesheetImage(root, "City.png"));

        var status = StatusOf(root);

        Assert.Contains("City.png", status.Warning, StringComparison.Ordinal);
        Assert.True(status.Missing.IsSheetMissing("city"));
    }

    [Fact]
    public void Refresh_AfterTheImageComesBack_ClearsTheWarning()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(TilesheetImage(root, "City.png"));
        var status = StatusOf(root);
        Assert.NotNull(status.Warning);
        File.Copy(TilesheetImage(_content.TestWorld, "City.png"), TilesheetImage(root, "City.png"));

        status.Refresh();

        Assert.Null(status.Warning);
        Assert.False(status.Missing.IsSheetMissing("city"));
    }

    private static ArtStatus StatusOf(string contentRoot) =>
        new(new CatalogDraft(ContentCatalogLoader.Load(contentRoot)), ArtFolders.RootIn(contentRoot));

    private static string TilesheetImage(string contentRoot, string file) =>
        Path.Combine(ArtFolders.RootIn(contentRoot), ArtFolders.Tilesheets, file);
}
