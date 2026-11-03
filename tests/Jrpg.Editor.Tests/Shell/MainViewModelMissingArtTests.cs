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

    // The eyedropper on a marked cell: the city sheet isn't in the sheet box, so the choice stays and nothing throws.
    [Fact]
    public void PickTileAt_TileOfMissingSheet_KeepsTheChoice()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        var viewModel = _content.Open(root);
        var map = viewModel.CurrentMap!;
        var cityCell = AllCells(map.Map.Layout).First(cell => map.TopTileAt(cell) is { } tile && viewModel.Catalog!.Tiles[tile.TileId].SheetId == "city");
        var choiceBefore = viewModel.Painting!.Browser.SelectedChoice;

        viewModel.Painting.PickTileAt(cityCell);

        Assert.Equal(choiceBefore, viewModel.Painting.Browser.SelectedChoice);
        Assert.Null(viewModel.Painting.Browser.SelectedCell);
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
