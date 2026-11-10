using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// Save and Save all writing in one go, all or none, each map checked with the open maps as they are now (decision
/// 518), on a copy of the test world. The deadlock: the forest resized so that start's warp into it had to move into
/// the new cells; each map's file refused against the other's saved one.
/// </summary>
public sealed class MainViewModelSaveTogetherTests : IDisposable
{
    private const string WarpEventId = "save_check";

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void SaveAll_ResizedMapAndTheWarpMovedIntoItsNewCells_WritesBoth()
    {
        var viewModel = OpenTheDeadlock();

        viewModel.SaveAllCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        var saved = ContentCatalogLoader.Load(_content.ContentCopy);
        Assert.Equal((29, 19), (saved.Maps["forest"].Layout.Width, saved.Maps["forest"].Layout.Height));
        Assert.Equal(new GridCell(0, 18), WarpIntoForestOn(saved.Maps["start"].Layout.CellEvents));
        Assert.False(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void Save_ShownMapNeedsAnotherOpenMapsChange_RefusesAndWritesNothing()
    {
        var viewModel = OpenTheDeadlock();
        var startBefore = _content.LayoutBytes("start");
        var forestBefore = _content.LayoutBytes("forest");

        viewModel.SaveCommand.Execute(null);

        Assert.StartsWith("map-layouts/start.json: ", viewModel.SaveError, StringComparison.Ordinal);
        Assert.EndsWith(" Nothing was written.", viewModel.SaveError, StringComparison.Ordinal);
        Assert.Equal(startBefore, _content.LayoutBytes("start"));
        Assert.Equal(forestBefore, _content.LayoutBytes("forest"));
    }

    // Removing an object the saved forest still places: the catalogs alone were refused against the saved map.
    [Fact]
    public void Save_CatalogChangeValidOnlyWithTheShownMapsChange_WritesBoth()
    {
        var viewModel = PlaceAndSaveANewObjectOnForest();
        RemoveThatObjectFromForestAndTheCatalog(viewModel);

        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        var saved = ContentCatalogLoader.Load(_content.ContentCopy);
        Assert.False(saved.Objects.ContainsKey("crate_4"));
        Assert.DoesNotContain(saved.Maps["forest"].Layout.Objects, placed => placed.ObjectId == "crate_4");
        Assert.False(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void SaveAll_MapRefusedAndTilesChanged_WritesTheTilesAlone()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var tileId = EditorTestContent.BlockFirstWalkableTile(viewModel);
        EditorTestContent.PaintUnknownTile(viewModel.CurrentMap!);

        viewModel.SaveAllCommand.Execute(null);

        AssertTilesWrittenAlone(viewModel, tileId);
    }

    [Fact]
    public void Save_MapRefusedAndTilesChanged_WritesTheTilesAlone()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var tileId = EditorTestContent.BlockFirstWalkableTile(viewModel);
        EditorTestContent.PaintUnknownTile(viewModel.CurrentMap!);

        viewModel.SaveCommand.Execute(null);

        AssertTilesWrittenAlone(viewModel, tileId);
    }

    // The catalogs pass only with the shown forest's change, and the forest is refused for its unknown tile.
    [Fact]
    public void Save_MapRefusedAndTilesRefusedAlone_WritesNothing()
    {
        var viewModel = PlaceAndSaveANewObjectOnForest();
        RemoveThatObjectFromForestAndTheCatalog(viewModel);
        EditorTestContent.PaintUnknownTile(viewModel.CurrentMap!);
        var objectsBefore = File.ReadAllBytes(_content.ObjectsPath);
        var forestBefore = _content.LayoutBytes("forest");

        viewModel.SaveCommand.Execute(null);

        Assert.StartsWith("map-layouts/forest.json: ", viewModel.SaveError, StringComparison.Ordinal);
        Assert.EndsWith(" Nothing was written.", viewModel.SaveError, StringComparison.Ordinal);
        Assert.Equal(objectsBefore, File.ReadAllBytes(_content.ObjectsPath));
        Assert.Equal(forestBefore, _content.LayoutBytes("forest"));
        Assert.True(viewModel.Content!.IsDirty);
    }

    private void AssertTilesWrittenAlone(MainViewModel viewModel, int tileId)
    {
        Assert.False(ContentCatalogLoader.Load(_content.ContentCopy).Tiles[tileId].IsWalkable);
        Assert.False(viewModel.Content!.IsDirty);
        Assert.True(viewModel.CurrentMap!.IsDirty);
        Assert.EndsWith(" The tiles and objects were saved; no map was written.", viewModel.SaveError, StringComparison.Ordinal);
    }

    // Start's warp into the forest's top-right cell, saved; then the forest narrowed by one column and grown by two
    // rows, and the warp moved to the forest's new bottom-left cell: outside the saved forest, inside the open one.
    private MainViewModel OpenTheDeadlock()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        viewModel.TryOpenMap("start");
        var start = viewModel.CurrentMap!;
        var cell = Enumerable.Range(0, start.Map.Layout.Width).Select(x => new GridCell(x, 3)).First(cell => start.Map.Layout.CellEvents.All(cellEvent => cellEvent.Cell != cell));
        SetWarpIntoForest(start, cell, new GridCell(29, 0));
        viewModel.SaveCommand.Execute(null);
        Assert.Null(viewModel.SaveError);

        viewModel.TryOpenMap("forest");
        var forest = viewModel.CurrentMap!;
        forest.Do(new ResizeMapEdit(forest.Map.Layout, 29, 19, viewModel.Catalog!.Objects));
        viewModel.TryOpenMap("start");
        SetWarpIntoForest(start, cell, new GridCell(0, 18));
        return viewModel;
    }

    private static void SetWarpIntoForest(MapViewModel start, GridCell cell, GridCell target)
    {
        var warp = new MapEvent { Id = WarpEventId, Activation = EventActivation.Touch, Steps = [new WarpStep("forest", target)] };
        start.Do(SetCellEventEdit.For(start.Map.Layout.CellEvents, cell, warp));
    }

    private static GridCell WarpIntoForestOn(IEnumerable<Core.Maps.CellEvent> cellEvents) =>
        ((WarpStep)cellEvents.Single(cellEvent => cellEvent.Event.Id == WarpEventId).Event.Steps[0]).Cell;

    // Objects > New object makes crate_4 from two sheet cells, placed on the shown forest and saved with it.
    private MainViewModel PlaceAndSaveANewObjectOnForest()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);
        EditorTestContent.ComposeObject(viewModel, "crate_4");
        viewModel.Placing!.PlaceObjectAt(new Point2D(2, 2));
        viewModel.SaveCommand.Execute(null);
        Assert.Null(viewModel.SaveError);
        return viewModel;
    }

    private static void RemoveThatObjectFromForestAndTheCatalog(MainViewModel viewModel)
    {
        var placing = viewModel.Placing!;
        placing.RemoveObject(viewModel.CurrentMap!.Map.Layout.Objects.Single(placed => placed.ObjectId == "crate_4").Uid);
        placing.Browser.SelectedObject = viewModel.Catalog!.Objects["crate_4"];
        placing.Browser.RemoveSelectedCommand.Execute(null);
        Assert.False(viewModel.Catalog!.Objects.ContainsKey("crate_4"));
    }
}
