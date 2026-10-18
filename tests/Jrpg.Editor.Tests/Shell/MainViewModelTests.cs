using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

public sealed class MainViewModelTests : IDisposable
{
    private static readonly string _repositoryContent = Path.Combine(AppContext.BaseDirectory, "content");

    private readonly string _contentCopy = Path.Combine(Path.GetTempPath(), $"jrpg-editor-{Guid.NewGuid():N}");
    private readonly List<string> _questions = [];

    public void Dispose()
    {
        if (Directory.Exists(_contentCopy))
        {
            Directory.Delete(_contentCopy, recursive: true);
        }
    }

    [Fact]
    public void Constructor_RepositoryContent_ListsMapsAndOpensTheFirst()
    {
        var viewModel = Open(_repositoryContent);

        Assert.Null(viewModel.LoadError);
        Assert.Equal(["forest", "interior_1", "start"], viewModel.MapIds);
        Assert.Equal("forest", viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void TryOpenMap_NoUnsavedChanges_OpensItWithoutAsking()
    {
        var viewModel = Open(_repositoryContent);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
        Assert.Empty(_questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedChangesAndUserKeepsThem_StaysOnTheMap()
    {
        var viewModel = Open(_repositoryContent, discardAnswer: false);
        PaintTopLeft(viewModel);

        Assert.False(viewModel.TryOpenMap("start"));

        Assert.Equal("forest", viewModel.CurrentMap?.Map.Id);
        Assert.Equal(["Map 'forest' has unsaved changes. Discard them?"], _questions);
    }

    [Fact]
    public void TryOpenMap_UnsavedChangesAndUserDiscards_OpensTheOther()
    {
        var viewModel = Open(_repositoryContent, discardAnswer: true);
        PaintTopLeft(viewModel);

        Assert.True(viewModel.TryOpenMap("start"));

        Assert.Equal("start", viewModel.CurrentMap?.Map.Id);
    }

    [Fact]
    public void ConfirmClose_UnsavedChanges_AsksTheUser()
    {
        var viewModel = Open(_repositoryContent, discardAnswer: false);
        PaintTopLeft(viewModel);

        Assert.False(viewModel.ConfirmClose());
        Assert.Single(_questions);
    }

    [Fact]
    public void ConfirmClose_MidStroke_AsksTheUser()
    {
        var viewModel = Open(_repositoryContent, discardAnswer: false);
        viewModel.Painting!.StartErasing(new GridCell(0, 0));

        Assert.False(viewModel.ConfirmClose());
        Assert.Single(_questions);
    }

    [Fact]
    public void Save_ValidEdit_WritesTheMapAndClearsUnsavedChanges()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var painted = PaintTopLeft(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.False(viewModel.CurrentMap!.IsDirty);
        Assert.Null(viewModel.SaveError);
        Assert.Equal(painted, ContentCatalogLoader.Load(_contentCopy).Maps["forest"].Layout.Layers[0].TileIdAt(new GridCell(0, 0)));
    }

    [Fact]
    public void Save_MapTheGameWouldReject_ShowsTheLoaderMessageAndStaysUnsaved()
    {
        var viewModel = Open(CopyOfRepositoryContent());
        var forest = viewModel.CurrentMap!;
        forest.Do(new SetTilesEdit(forest.Map.Layout.Layers[0].Z, [new TileChange(new GridCell(0, 0), forest.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0)), 99_999)]));

        viewModel.SaveCommand.Execute(null);

        Assert.StartsWith("map-layouts/forest.json: ", viewModel.SaveError, StringComparison.Ordinal);
        Assert.True(forest.IsDirty);
    }

    [Fact]
    public void ShowCursorAt_CellOnTheMap_NamesTheCellAndItsTopTile()
    {
        var viewModel = Open(_repositoryContent);
        var tile = viewModel.CurrentMap!.TopTileAt(new GridCell(3, 2))!.Value;

        viewModel.ShowCursorAt(new GridCell(3, 2));

        Assert.Equal($"Cell (3, 2)   Tile {tile.TileId} on layer z {tile.LayerZ}", viewModel.CursorStatus);
    }

    [Fact]
    public void ShowCursorAt_CellWithoutTiles_SaysSo()
    {
        // The start map's one hole, left by the legacy editor (seen in slice 3a).
        var viewModel = Open(_repositoryContent);
        viewModel.TryOpenMap("start");

        viewModel.ShowCursorAt(new GridCell(5, 13));

        Assert.Equal("Cell (5, 13)   No tile", viewModel.CursorStatus);
    }

    [Fact]
    public void ShowCursorAt_CellOffTheMap_ClearsTheStatus()
    {
        var viewModel = Open(_repositoryContent);
        viewModel.ShowCursorAt(new GridCell(3, 2));

        viewModel.ShowCursorAt(new GridCell(-1, 2));

        Assert.Equal("", viewModel.CursorStatus);
    }

    [Fact]
    public void TryOpenMap_AnotherMap_HandsItToPaintingAndPlacing()
    {
        var viewModel = Open(_repositoryContent);

        viewModel.TryOpenMap("start");

        Assert.Same(viewModel.CurrentMap, viewModel.Painting!.OpenMap);
        Assert.Same(viewModel.CurrentMap, viewModel.Placing!.OpenMap);
    }

    [Fact]
    public void Constructor_BrokenContentFile_ShowsTheLoaderMessageAndNoMaps()
    {
        Directory.CreateDirectory(_contentCopy);
        File.WriteAllText(Path.Combine(_contentCopy, "classes.json"), "[ {");

        var viewModel = Open(_contentCopy);

        Assert.StartsWith("classes.json", viewModel.LoadError, StringComparison.Ordinal);
        Assert.Empty(viewModel.MapIds);
        Assert.Null(viewModel.CurrentMap);
        Assert.Null(viewModel.Painting);
        Assert.Null(viewModel.Placing);
    }

    private MainViewModel Open(string contentRoot, bool discardAnswer = false) =>
        new(contentRoot, question =>
        {
            _questions.Add(question);
            return discardAnswer;
        });

    // Paints the open map's top-left ground cell with another tile already used on the map, so the result is valid content.
    private static int PaintTopLeft(MainViewModel viewModel)
    {
        var map = viewModel.CurrentMap!;
        var ground = map.Map.Layout.Layers[0];
        var before = ground.TileIdAt(new GridCell(0, 0));
        var after = ground.Rows.SelectMany(row => row).First(id => id != before && id != 0);
        map.Do(new SetTilesEdit(ground.Z, [new TileChange(new GridCell(0, 0), before, after)]));
        return after;
    }

    private string CopyOfRepositoryContent()
    {
        foreach (var file in Directory.EnumerateFiles(_repositoryContent, "*", SearchOption.AllDirectories))
        {
            var target = Path.Combine(_contentCopy, Path.GetRelativePath(_repositoryContent, file));
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.Copy(file, target);
        }

        return _contentCopy;
    }
}
