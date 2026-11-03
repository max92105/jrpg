using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Editor.Tests.Shell;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// Objects > Replace cells of the chosen object (decision 449), through the editor on a copy of the test world.
/// </summary>
public sealed class ObjectReskinningTests : IDisposable
{
    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Reskin_CellsNotTilesYet_MakesThemTiles()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var cells = _content.ChooseTwoCellsNotTiles(viewModel);
        ChooseObject(viewModel, "chest");

        viewModel.Reskinning!.ReskinCommand.Execute(null);

        Assert.All(cells.Cells, cell => Assert.NotNull(viewModel.Content!.TileAt(cell)));
        Assert.Equal(cells.Cells.Select(cell => viewModel.Content!.TileAt(cell)!.Id), viewModel.Catalog!.Objects["chest"].Tiles.Select(tile => tile.TileId));
    }

    // The chest's placements and their events (give a potion, swap to the open chest) stay as they were.
    [Fact]
    public void Reskin_PlacedObjectWithEvents_KeepsThePlacementsAndEvents()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var placementsBefore = ChestPlacements(viewModel);
        var eventsBefore = placementsBefore.SelectMany(placed => placed.Events).ToList();
        Assert.NotEmpty(eventsBefore);
        _content.ChooseTwoCellsNotTiles(viewModel);
        ChooseObject(viewModel, "chest");

        viewModel.Reskinning!.ReskinCommand.Execute(null);

        Assert.Equal(placementsBefore, ChestPlacements(viewModel));
        Assert.Equal(eventsBefore, ChestPlacements(viewModel).SelectMany(placed => placed.Events));
        Assert.Equal((2, 1), (viewModel.Catalog!.Objects["chest"].Width, viewModel.Catalog.Objects["chest"].Height));
    }

    [Fact]
    public void Reskin_Undo_BringsTheOldPictureBack()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        var before = viewModel.Catalog!.Objects["chest"];
        _content.ChooseTwoCellsNotTiles(viewModel);
        ChooseObject(viewModel, "chest");
        viewModel.Reskinning!.ReskinCommand.Execute(null);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Same(before, viewModel.Catalog!.Objects["chest"]);
    }

    [Fact]
    public void CanReskin_NpcChosen_IsFalse()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);

        ChooseObject(viewModel, "villager");

        Assert.False(viewModel.Reskinning!.ReskinCommand.CanExecute(null));
    }

    [Fact]
    public void CanReskin_NothingChosenInTheTiles_IsFalse()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());

        ChooseObject(viewModel, "chest");

        Assert.Null(viewModel.Painting!.Browser.MarkedCells);
        Assert.False(viewModel.Reskinning!.ReskinCommand.CanExecute(null));
    }

    [Fact]
    public void CanReskin_ObjectAndCellsChosen_IsTrue()
    {
        var viewModel = _content.Open(_content.CopyOfTestWorld());
        _content.ChooseTwoCellsNotTiles(viewModel);

        ChooseObject(viewModel, "chest");

        Assert.True(viewModel.Reskinning!.ReskinCommand.CanExecute(null));
    }

    // A city cell picked off the map with City.png gone, then another sheet shown: the cells are still on the city
    // sheet.
    [Fact]
    public void CanReskin_CellsOnASheetWhoseImageIsMissing_IsFalseWhateverSheetIsShown()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        var viewModel = _content.Open(root);
        var browser = viewModel.Painting!.Browser;
        browser.Select(viewModel.Catalog!.Tiles.Values.First(tile => tile.SheetId == "city").Id);
        ChooseObject(viewModel, "chest");

        browser.SelectedChoice = browser.SheetChoices.First(choice => choice.Sheet is { } sheet && sheet.Id != "city");

        Assert.Equal("city", browser.MarkedCells!.Value.SheetId);
        Assert.False(viewModel.Reskinning!.ReskinCommand.CanExecute(null));
    }

    // Cells marked on a sheet with its image, then a sheet whose image is missing shown: the marked cells are fine.
    [Fact]
    public void CanReskin_CellsOnAWholeSheetWhileAMissingOneIsShown_IsTrue()
    {
        var root = _content.CopyOfTestWorld();
        File.Delete(Path.Combine(ArtFolders.RootIn(root), ArtFolders.Tilesheets, "City.png"));
        var viewModel = _content.Open(root);
        _content.ChooseTwoCellsNotTiles(viewModel);
        ChooseObject(viewModel, "chest");
        var browser = viewModel.Painting!.Browser;

        browser.SelectedChoice = browser.SheetChoices.Single(choice => choice.IsImageMissing);

        Assert.True(viewModel.Reskinning!.ReskinCommand.CanExecute(null));
    }

    private static void ChooseObject(Editor.Shell.MainViewModel viewModel, string objectId)
    {
        var browser = viewModel.Placing!.Browser;
        browser.SelectedObject = browser.Objects.First(definition => definition.Id == objectId);
    }

    private static List<PlacedObject> ChestPlacements(Editor.Shell.MainViewModel viewModel) =>
        [.. viewModel.Catalog!.Maps.Values.SelectMany(map => map.Layout.Objects).Where(placed => placed.ObjectId == "chest")];
}
