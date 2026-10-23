using Jrpg.Core.Geometry;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>Undo and redo across map and catalog edits, on a copy of the repository's content.</summary>
public sealed class MainViewModelUndoTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Undo_BlockedTileMarkThenPaint_UndoesThePaintFirst()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var blockedId = EditorTestContent.BlockFirstWalkableTile(viewModel);
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.UndoCommand.Execute(null);

        Assert.False(viewModel.CurrentMap!.IsDirty);
        Assert.False(viewModel.Catalog!.Tiles[blockedId].IsWalkable);
        viewModel.UndoCommand.Execute(null);
        Assert.True(viewModel.Catalog!.Tiles[blockedId].IsWalkable);
        Assert.False(viewModel.HasUnsavedChanges);
    }

    [Fact]
    public void Redo_AfterUndoingMarkAndPaint_RedoesTheMarkFirst()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var blockedId = EditorTestContent.BlockFirstWalkableTile(viewModel);
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.UndoCommand.Execute(null);
        viewModel.UndoCommand.Execute(null);

        viewModel.RedoCommand.Execute(null);

        Assert.False(viewModel.Catalog!.Tiles[blockedId].IsWalkable);
        Assert.False(viewModel.CurrentMap!.IsDirty);
        viewModel.RedoCommand.Execute(null);
        Assert.True(viewModel.CurrentMap.IsDirty);
    }

    [Fact]
    public void Redo_NewTilePaintedOffTheMapAfterUndo_HasNothingToRedo()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        viewModel.Content!.AddBlockedTile(new SheetCell(viewModel.Catalog!.Tilesheets.Keys.First(), 999, 999));
        viewModel.UndoCommand.Execute(null);
        _content.ChooseCellNotATile(viewModel);

        viewModel.Painting!.PaintRectangle(new GridCell(-5, -5), new GridCell(-4, -4));

        Assert.False(viewModel.RedoCommand.CanExecute(null));
    }

    [Fact]
    public void Undo_NewBlockedTileUsedOnAnotherMap_RefusesAndNamesTheMap()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        var cell = _content.ChooseCellNotATile(viewModel);
        viewModel.Content!.AddBlockedTile(cell);
        var tileId = viewModel.Content.TileAt(cell)!.Id;
        viewModel.Painting!.StartPainting(new GridCell(0, 0));
        viewModel.Painting.EndStroke();
        var paintedMapId = viewModel.CurrentMap!.Map.Id;
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != paintedMapId));

        viewModel.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: tile {tileId} is used on map {paintedMapId}.", viewModel.UndoRefusal);
        Assert.NotNull(viewModel.Content.TileAt(cell));
        Assert.False(viewModel.RedoCommand.CanExecute(null));
    }

    [Fact]
    public void Undo_SetUpSheetWithTilesCut_RefusesNamingTheTile()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var browser = viewModel.Painting!.Browser;
        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.File == "Grass3.png");
        browser.SetUpSheetCommand.Execute(null);
        browser.ApplyCutCommand.Execute(null);
        browser.SelectCellAt(1, 1);
        var tileId = viewModel.Content!.AddTile(browser.SelectedCell!.Value);

        viewModel.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: tile {tileId} is cut from sheet grass_3.", viewModel.UndoRefusal);
        Assert.True(viewModel.Catalog!.Tilesheets.ContainsKey("grass_3"));
    }

    [Fact]
    public void Undo_RefusedThenAnEdit_ClearsTheRefusal()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        var cell = _content.ChooseCellNotATile(viewModel);
        viewModel.Content!.AddBlockedTile(cell);
        viewModel.Painting!.StartPainting(new GridCell(0, 0));
        viewModel.Painting.EndStroke();
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));
        viewModel.UndoCommand.Execute(null);

        EditorTestContent.BlockFirstWalkableTile(viewModel);

        Assert.Null(viewModel.UndoRefusal);
    }

    [Fact]
    public void Undo_CatalogStepAfterSwitchingMaps_UndoesIt()
    {
        var viewModel = _content.Open(_content.RepositoryContent);
        var blockedId = EditorTestContent.BlockFirstWalkableTile(viewModel);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));

        viewModel.UndoCommand.Execute(null);

        Assert.True(viewModel.Catalog!.Tiles[blockedId].IsWalkable);
    }

    [Fact]
    public void Undo_NewObjectPlacedOnASavedMap_RefusesAndNamesTheMap()
    {
        var viewModel = _content.Open(_content.CopyOfRepositoryContent());
        _content.ChooseTwoCellsNotTiles(viewModel);
        var newObject = viewModel.StartNewObject()!;
        newObject.Id = "tree_4";
        newObject.Category = "General";
        viewModel.CreateObject(newObject);
        viewModel.Placing!.PlaceObjectAt(new Point2D(2, 2));
        var placedOn = viewModel.CurrentMap!.Map.Id;
        viewModel.SaveCommand.Execute(null);
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != placedOn));

        viewModel.UndoCommand.Execute(null);

        Assert.Equal($"Can't undo: object tree_4 is placed on map {placedOn}.", viewModel.UndoRefusal);
        Assert.True(viewModel.Catalog!.Objects.ContainsKey("tree_4"));
    }
}
