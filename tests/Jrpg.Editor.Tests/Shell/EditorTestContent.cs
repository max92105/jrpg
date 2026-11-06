using Jrpg.Core.Geometry;
using Jrpg.Data.Art;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// The main view model tests' content: the test world (ADR 0037), or a temporary copy of it deleted on dispose, the
/// questions the view model asked, and the editing steps several tests share.
/// </summary>
internal sealed class EditorTestContent : IDisposable
{
    public string TestWorld { get; } = Path.Combine(AppContext.BaseDirectory, "test-world");

    public string ContentCopy { get; } = Path.Combine(Path.GetTempPath(), $"jrpg-editor-{Guid.NewGuid():N}");

    /// <summary>Every question the view model asked, in order.</summary>
    public List<string> Questions { get; } = [];

    public void Dispose()
    {
        if (Directory.Exists(ContentCopy))
        {
            Directory.Delete(ContentCopy, recursive: true);
        }
    }

    /// <param name="contentRoot">The test world, or <see cref="ContentCopy"/> for a test that saves.</param>
    /// <param name="answer">
    /// What the user answers to every yes or no question; the close question gets Don't save for yes, Cancel for no.
    /// </param>
    public MainViewModel Open(string contentRoot, bool answer = false) =>
        Open(contentRoot, answer, answer ? UnsavedChangesAnswer.Discard : UnsavedChangesAnswer.Cancel);

    /// <param name="contentRoot">The test world, or <see cref="ContentCopy"/> for a test that saves.</param>
    /// <param name="closeAnswer">What the user answers to the close question; no to every yes or no question.</param>
    public MainViewModel Open(string contentRoot, UnsavedChangesAnswer closeAnswer) => Open(contentRoot, answer: false, closeAnswer);

    private MainViewModel Open(string contentRoot, bool answer, UnsavedChangesAnswer closeAnswer) =>
        new(
            contentRoot,
            question =>
            {
                Questions.Add(question);
                return answer;
            },
            question =>
            {
                Questions.Add(question);
                return closeAnswer;
            });

    // Paints the open map's top-left ground cell with another tile already used on the map, so the result is valid content.
    public static int PaintTopLeft(MainViewModel viewModel)
    {
        var map = viewModel.CurrentMap!;
        var ground = map.Map.Layout.Layers[0];
        var before = ground.TileIdAt(new GridCell(0, 0));
        var after = ground.Rows.SelectMany(row => row).First(id => id != before && id != 0);
        map.Do(new SetTilesEdit(ground.Z, [new TileChange(new GridCell(0, 0), before, after)]));
        return after;
    }

    // Chooses, with Shift + drag, the first two cells side by side of the shown sheet that aren't tiles yet.
    public SheetCellRectangle ChooseTwoCellsNotTiles(MainViewModel viewModel)
    {
        var browser = viewModel.Painting!.Browser;
        var sheet = browser.SelectedSheet!;
        var (width, height) = PngSize.Read(Path.Combine(ArtFolders.RootIn(ContentCopy), ArtFolders.Tilesheets, sheet.File));
        var left = (from row in Enumerable.Range(0, sheet.RowsIn(height))
                    from column in Enumerable.Range(0, sheet.ColumnsIn(width) - 1)
                    select new SheetCell(sheet.Id, column, row))
            .First(cell => viewModel.Content!.TileAt(cell) is null && viewModel.Content.TileAt(cell with { Column = cell.Column + 1 }) is null);
        browser.SelectCellsBetween(
            new Point2D(sheet.CellLeft(left.Column), sheet.CellTop(left.Row)),
            new Point2D(sheet.CellLeft(left.Column + 1), sheet.CellTop(left.Row)));
        return browser.SelectedCells!.Value;
    }

    // Clicks the first cell of the shown sheet that isn't a tile yet, sized by its image in the content copy.
    public SheetCell ChooseCellNotATile(MainViewModel viewModel)
    {
        var browser = viewModel.Painting!.Browser;
        var sheet = browser.SelectedSheet!;
        var (width, height) = PngSize.Read(Path.Combine(ArtFolders.RootIn(ContentCopy), ArtFolders.Tilesheets, sheet.File));
        var cell = (from row in Enumerable.Range(0, sheet.RowsIn(height))
                    from column in Enumerable.Range(0, sheet.ColumnsIn(width))
                    select new SheetCell(sheet.Id, column, row)).First(cell => viewModel.Content!.TileAt(cell) is null);
        browser.SelectCellAt(sheet.CellLeft(cell.Column), sheet.CellTop(cell.Row));
        return cell;
    }

    // Objects > New object over the cells chosen in the Tiles panel, saved with this id in the General category.
    public static void ComposeObject(MainViewModel viewModel, string id)
    {
        var composer = viewModel.Composing!.StartNew();
        composer.Naming.Id = id;
        composer.Naming.Category = "General";
        Assert.True(viewModel.Composing.TryApply(composer));
    }

    public static int BlockFirstWalkableTile(MainViewModel viewModel)
    {
        var tileId = viewModel.Catalog!.Tiles.Values.First(tile => tile.IsWalkable).Id;
        viewModel.Content!.SetWalkable(tileId, isWalkable: false);
        return tileId;
    }

    public byte[] TilesAndForestBytes() =>
        [.. File.ReadAllBytes(Path.Combine(ContentCopy, "tiles.json")),
         .. File.ReadAllBytes(Path.Combine(ContentCopy, "map-layouts", "forest.json"))];

    public string CopyOfTestWorld()
    {
        foreach (var file in Directory.EnumerateFiles(TestWorld, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(ContentCopy, Path.GetRelativePath(TestWorld, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }

        return ContentCopy;
    }
}
