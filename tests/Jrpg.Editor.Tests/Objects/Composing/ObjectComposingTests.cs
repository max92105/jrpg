using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects.Composing;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Tests.Shell;

namespace Jrpg.Editor.Tests.Objects.Composing;

/// <summary>Objects > New object and Edit object through the editor, on a copy of the test world.</summary>
public sealed class ObjectComposingTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void StartNew_RectangleChosenInTheTiles_StartsTheCanvasWithIt()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var cells = _content.ChooseTwoCellsNotTiles(viewModel);

        var canvas = viewModel.Composing!.StartNew().Painting.Canvas;

        Assert.Equal((2, 1), (canvas.Width, canvas.Height));
        Assert.Equal(ObjectCanvas.Of(cells).Cells, canvas.Cells);
    }

    // The jungle sheet has 47 rows: a drag down 40 of them starts the canvas with the top 32.
    [Fact]
    public void StartNew_RectangleTallerThanMaxCells_StartsWithItsTopCells()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var browser = viewModel.Painting!.Browser;
        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.Sheet?.Id == "ra_jungle");
        var sheet = browser.SelectedSheet!;
        browser.SelectCellsBetween(new Point2D(sheet.CellLeft(0), sheet.CellTop(0)), new Point2D(sheet.CellLeft(0), sheet.CellTop(39)));

        var canvas = viewModel.Composing!.StartNew().Painting.Canvas;

        Assert.Equal((1, ObjectCanvas.MaxCells), (canvas.Width, canvas.Height));
        Assert.Equal(new SheetCell("ra_jungle", 0, ObjectCanvas.MaxCells - 1), canvas.Cells[new GridCell(0, ObjectCanvas.MaxCells - 1)]);
    }

    [Fact]
    public void StartNew_NoRectangleChosen_StartsAnEmptyThreeByThreeCanvas()
    {
        var canvas = _content.Open(_content.TestWorld).Composing!.StartNew().Painting.Canvas;

        Assert.Equal((3, 3), (canvas.Width, canvas.Height));
        Assert.True(canvas.IsBlank);
    }

    [Fact]
    public void StartEdit_TileObjectChosen_StartsWithItsCellsAndItsId()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var chest = ChooseObject(viewModel, "chest");

        var composer = viewModel.Composing!.StartEdit()!;

        Assert.Same(chest, composer.Original);
        Assert.Equal(("chest", false), (composer.Naming.Id, composer.IsIdEditable));
        Assert.Equal(ObjectCanvas.Of(chest, viewModel.Catalog!.Tiles).Cells, composer.Painting.Canvas.Cells);
    }

    [Fact]
    public void CanEdit_NpcChosen_IsFalse()
    {
        var viewModel = _content.Open(_content.TestWorld);

        ChooseObject(viewModel, "villager");

        Assert.False(viewModel.Composing!.CanEdit);
        Assert.Null(viewModel.Composing.StartEdit());
    }

    [Fact]
    public void CanEdit_NothingChosen_IsFalse() => Assert.False(_content.Open(_content.TestWorld).Composing!.CanEdit);

    [Fact]
    public void CanEdit_TileObjectChosen_IsTrueAndRaised()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var raised = new List<string?>();
        viewModel.Composing!.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        ChooseObject(viewModel, "chest");

        Assert.True(viewModel.Composing.CanEdit);
        Assert.Contains(nameof(ObjectComposing.CanEdit), raised);
    }

    [Fact]
    public void CanEdit_ObjectTallerThanMaxCells_IsFalse()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var chest = viewModel.Catalog!.Objects["chest"];
        viewModel.Content!.SetObject(chest with { Id = "tower", Height = ObjectCanvas.MaxCells + 1 });

        ChooseObject(viewModel, "tower");

        Assert.False(viewModel.Composing!.CanEdit);
        Assert.Null(viewModel.Composing.StartEdit());
    }

    // A new object nothing uses yet, so Remove takes it out (decision 443).
    [Fact]
    public void CanEdit_AfterTheChosenObjectIsRemoved_IsFalse()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);
        EditorTestContent.ComposeObject(viewModel, "tree_4");
        Assert.True(viewModel.Composing!.CanEdit);

        viewModel.Placing!.Browser.RemoveSelectedCommand.Execute(null);

        Assert.False(viewModel.Catalog!.Objects.ContainsKey("tree_4"));
        Assert.False(viewModel.Composing.CanEdit);
    }

    [Fact]
    public void TryApply_EditWithNoChange_KeepsTheStoredObjectChosen()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        ChooseObject(viewModel, "chest");

        Assert.True(viewModel.Composing!.TryApply(viewModel.Composing.StartEdit()!));

        Assert.Same(viewModel.Catalog!.Objects["chest"], viewModel.Placing!.Browser.SelectedObject);
    }

    [Fact]
    public void TryApply_NewWithCellsNotTilesYet_MakesTheTilesAndTheObjectThenChoosesIt()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var cells = _content.ChooseTwoCellsNotTiles(viewModel);
        var composer = Named(viewModel.Composing!.StartNew(), "tree_4");

        Assert.True(viewModel.Composing.TryApply(composer));
        viewModel.SaveCommand.Execute(null);

        Assert.Null(viewModel.SaveError);
        Assert.Equal("tree_4", viewModel.Placing!.Browser.SelectedObject?.Id);
        var saved = ContentCatalogLoader.Load(_content.ContentCopy);
        var tree = saved.Objects["tree_4"];
        Assert.Equal(("General", 2, 1), (tree.Category, tree.Width, tree.Height));
        Assert.Equal(cells.Cells, tree.Tiles.Select(tile => SheetCellOf(saved.Tiles[tile.TileId])));
    }

    [Fact]
    public void TryApply_NewWithATakenId_ReturnsFalseAndKeepsTheObject()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var chest = viewModel.Catalog!.Objects["chest"];
        _content.ChooseTwoCellsNotTiles(viewModel);
        var composer = Named(viewModel.Composing!.StartNew(), "chest");

        Assert.False(viewModel.Composing.TryApply(composer));
        Assert.Same(chest, viewModel.Catalog!.Objects["chest"]);
    }

    [Fact]
    public void TryApply_Undo_RemovesTheNewObjectAsOneStep()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);
        viewModel.Composing!.TryApply(Named(viewModel.Composing.StartNew(), "tree_4"));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.False(viewModel.Catalog!.Objects.ContainsKey("tree_4"));
    }

    // The chest's placements and their events (give a potion, swap to the open chest) stay as they were.
    [Fact]
    public void TryApply_EditOfAPlacedObjectWithEvents_KeepsThePlacementsAndEvents()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var placementsBefore = ChestPlacements(viewModel);
        Assert.NotEmpty(placementsBefore.SelectMany(placed => placed.Events));
        var composer = EditChestWithTwoNewCells(viewModel);

        Assert.True(viewModel.Composing!.TryApply(composer));

        Assert.Equal(placementsBefore, ChestPlacements(viewModel));
        var chest = viewModel.Catalog!.Objects["chest"];
        Assert.Equal(composer.Painting.Canvas.Cells, chest.Tiles.ToDictionary(tile => new GridCell(tile.X, tile.Y), tile => SheetCellOf(viewModel.Catalog.Tiles[tile.TileId])));
    }

    [Fact]
    public void TryApply_EditUndo_BringsTheOldPictureBack()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var before = viewModel.Catalog!.Objects["chest"];
        viewModel.Composing!.TryApply(EditChestWithTwoNewCells(viewModel));

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Same(before, viewModel.Catalog!.Objects["chest"]);
    }

    // The developer's case: parts from anywhere, here one tile of the city sheet and one of the ground sheet.
    [Fact]
    public void TryApply_ScatteredCellsFromTwoSheets_MakesOneObjectOfBoth()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var composer = Named(viewModel.Composing!.StartNew(), "well");
        var city = PaintTileOf(viewModel, composer, "city", new GridCell(0, 0));
        var ground = PaintTileOf(viewModel, composer, "ground", new GridCell(2, 2));

        Assert.True(viewModel.Composing.TryApply(composer));

        Assert.Equal([new ObjectTile(0, 0, city), new ObjectTile(2, 2, ground)], viewModel.Catalog!.Objects["well"].Tiles);
    }

    private static ObjectComposerViewModel Named(ObjectComposerViewModel composer, string id)
    {
        composer.Naming.Id = id;
        composer.Naming.Category = "General";
        return composer;
    }

    // Opens the chest in the composer, then stamps two cells not tiles yet over its top-left.
    private ObjectComposerViewModel EditChestWithTwoNewCells(MainViewModel viewModel)
    {
        ChooseObject(viewModel, "chest");
        var composer = viewModel.Composing!.StartEdit()!;
        _content.ChooseTwoCellsNotTiles(viewModel);
        Stroke(composer, new GridCell(0, 0));
        return composer;
    }

    private static int PaintTileOf(MainViewModel viewModel, ObjectComposerViewModel composer, string sheetId, GridCell at)
    {
        var tileId = viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId == sheetId).Id;
        viewModel.Painting!.Browser.Select(tileId);
        Stroke(composer, at);
        return tileId;
    }

    private static void Stroke(ObjectComposerViewModel composer, GridCell at)
    {
        composer.Painting.BeginStroke();
        composer.Painting.PaintAt(at);
        composer.Painting.EndStroke();
    }

    private static ObjectDefinition ChooseObject(MainViewModel viewModel, string objectId)
    {
        var browser = viewModel.Placing!.Browser;
        browser.SelectedObject = browser.Objects.First(definition => definition.Id == objectId);
        return browser.SelectedObject;
    }

    private static List<PlacedObject> ChestPlacements(MainViewModel viewModel) =>
        [.. viewModel.Catalog!.Maps.Values.SelectMany(map => map.Layout.Objects).Where(placed => placed.ObjectId == "chest")];

    private static SheetCell SheetCellOf(TileDefinition tile) => new(tile.SheetId, tile.Column, tile.Row);
}
