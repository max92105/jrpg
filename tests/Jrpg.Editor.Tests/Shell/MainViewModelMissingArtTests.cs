using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>The editor opens with missing art (decision 442); a broken content file still locks it out.</summary>
public sealed class MainViewModelMissingArtTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Constructor_SheetImageDeleted_OpensTheMapsWithAWarning()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));

        var viewModel = _content.Open(root);

        Assert.Null(viewModel.LoadError);
        Assert.Contains("start", viewModel.MapIds);
        Assert.NotNull(viewModel.Painting);
        Assert.Contains("City.png", viewModel.Art!.Warning, StringComparison.Ordinal);
    }

    // The eyedropper on a marked cell shows the sheet as "image missing", and the cursor names it.
    [Fact]
    public void PickTileAt_TileOfMissingSheet_ShowsTheMissingSheet()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        var viewModel = _content.Open(root);
        var map = viewModel.CurrentMap!;
        var cityCell = AllCells(map.Map.Layout).First(cell => map.TopTileAt(cell) is { } tile && viewModel.Catalog!.Tiles[tile.TileId].SheetId == "city");

        viewModel.Painting!.PickTileAt(cityCell);
        viewModel.ShowCursorAt(cityCell);

        Assert.Equal("city (image missing)", viewModel.Painting.Browser.SelectedChoice!.Label);
        Assert.EndsWith("(sheet city: image missing)", viewModel.CursorStatus, StringComparison.Ordinal);
    }

    // A file that is there but isn't a PNG (a copy cut short, another format renamed) counts as missing in the sheet
    // box and in the drawing alike, instead of crashing the first size read or draw.
    [Fact]
    public void Constructor_SheetImageUnreadable_ListsTheSheetAsMissing()
    {
        var root = _content.CopyOfTestWorld();
        File.WriteAllText(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"), "not a png");

        var viewModel = _content.Open(root);

        Assert.Contains("city (image missing)", viewModel.Painting!.Browser.SheetChoices.Select(choice => choice.Label));
        Assert.True(viewModel.Art!.Missing.IsSheetMissing("city"));
        Assert.Contains("not a PNG image", viewModel.Art.Warning, StringComparison.Ordinal);
    }

    [Fact]
    public void ImportSheetImages_MissingSheetsImage_ClearsTheWarningWithoutAsking()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        var viewModel = _content.Open(root);
        Assert.NotNull(viewModel.Art!.Warning);

        viewModel.ImportSheetImages([Path.Combine(ArtFolders.RootIn(_content.TestWorld), ArtFolders.Tilesheets, "City.png")]);

        Assert.Null(viewModel.Art.Warning);
        Assert.Empty(_content.Questions);
        Assert.Equal("city", viewModel.Painting!.Browser.SelectedChoice!.Label);
    }

    [Fact]
    public void Save_SheetImageMissing_SavesAndKeepsTheWarning()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        var viewModel = _content.Open(root);
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.False(viewModel.HasUnsavedChanges);
        Assert.Contains("City.png", viewModel.Art!.Warning, StringComparison.Ordinal);
    }

    [Fact]
    public void Constructor_BrokenContentFile_StillShowsLoadError()
    {
        Directory.CreateDirectory(_content.ContentCopy);
        File.WriteAllText(Path.Combine(_content.ContentCopy, "skills.json"), "[ {");

        var viewModel = _content.Open(_content.ContentCopy);

        Assert.NotNull(viewModel.LoadError);
        Assert.Null(viewModel.Art);
    }

    private static IEnumerable<GridCell> AllCells(MapLayout layout) =>
        from y in Enumerable.Range(0, layout.Height)
        from x in Enumerable.Range(0, layout.Width)
        select new GridCell(x, y);
}
