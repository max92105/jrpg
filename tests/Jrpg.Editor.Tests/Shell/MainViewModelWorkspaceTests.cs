using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Trade;
using Jrpg.Editor.Shell;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Tests.Shell;

/// <summary>
/// The mode bar on the test world (decisions 916, 925): World and Database, the map's Delete selected objects and undo
/// steps out of reach in Database, Back to previous map showing World first.
/// </summary>
public sealed class MainViewModelWorkspaceTests : IDisposable
{
    private static readonly ShopDefinition _armory = new("armory", "Armory", ["bronze_sword"]);

    private readonly EditorTestContent _content = new();

    public void Dispose() => _content.Dispose();

    [Fact]
    public void Workspace_EditorOpened_IsWorld()
    {
        var viewModel = _content.Open(_content.TestWorld);

        Assert.Equal(EditorWorkspace.World, viewModel.Workspace);
        Assert.True(viewModel.IsWorldShown);
        Assert.False(viewModel.IsDatabaseShown);
    }

    [Fact]
    public void ShowDatabase_FromWorld_ShowsDatabase()
    {
        var viewModel = _content.Open(_content.TestWorld);

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.Equal(EditorWorkspace.Database, viewModel.Workspace);
        Assert.False(viewModel.IsWorldShown);
        Assert.True(viewModel.IsDatabaseShown);
    }

    [Fact]
    public void ShowDatabase_FromWorld_RaisesIsWorldShownAndIsDatabaseShown()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var raised = new List<string?>();
        viewModel.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.Contains(nameof(MainViewModel.IsWorldShown), raised);
        Assert.Contains(nameof(MainViewModel.IsDatabaseShown), raised);
    }

    [Fact]
    public void ShowWorld_FromDatabase_ShowsWorld()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.ShowDatabaseCommand.Execute(null);

        viewModel.ShowWorldCommand.Execute(null);

        Assert.Equal(EditorWorkspace.World, viewModel.Workspace);
        Assert.False(viewModel.IsDatabaseShown);
    }

    [Fact]
    public void ShowDatabase_ObjectsSelected_GreysDeleteSelectedObjects()
    {
        var viewModel = OpenWithFirstObjectSelected();

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.False(viewModel.Placing!.DeleteSelectedObjectsCommand.CanExecute(null));
    }

    [Fact]
    public void ShowDatabase_ObjectsSelected_RaisesDeleteSelectedObjectsCanExecuteChanged()
    {
        var viewModel = OpenWithFirstObjectSelected();
        var raised = 0;
        viewModel.Placing!.DeleteSelectedObjectsCommand.CanExecuteChanged += (_, _) => raised++;

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.True(raised > 0);
    }

    [Fact]
    public void ShowWorld_ObjectsSelected_LightsDeleteSelectedObjectsAgain()
    {
        var viewModel = OpenWithFirstObjectSelected();
        viewModel.ShowDatabaseCommand.Execute(null);

        viewModel.ShowWorldCommand.Execute(null);

        Assert.True(viewModel.Placing!.DeleteSelectedObjectsCommand.CanExecute(null));
    }

    [Fact]
    public void ShowWorld_ObjectsSelected_KeepsTheSelection()
    {
        var viewModel = OpenWithFirstObjectSelected();
        var selected = viewModel.Placing!.SelectedObjectUids.Single();
        viewModel.ShowDatabaseCommand.Execute(null);

        viewModel.ShowWorldCommand.Execute(null);

        Assert.Equal([selected], viewModel.Placing.SelectedObjectUids);
    }

    [Fact]
    public void GoBackToMap_DatabaseShown_ShowsWorldAndGoesBack()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var firstMapId = viewModel.CurrentMap!.Map.Id;
        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != firstMapId));
        viewModel.ShowDatabaseCommand.Execute(null);

        viewModel.GoBackToMapCommand.Execute(null);

        Assert.Equal((EditorWorkspace.World, firstMapId), (viewModel.Workspace, viewModel.CurrentMap!.Map.Id));
    }

    [Fact]
    public void GoBackToMapCommand_NoPreviousMap_IsGreyed()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.False(viewModel.GoBackToMapCommand.CanExecute(null));
    }

    [Fact]
    public void GoBackToMapCommand_SecondMapOpened_RaisesCanExecuteChanged()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var raised = 0;
        viewModel.GoBackToMapCommand.CanExecuteChanged += (_, _) => raised++;

        viewModel.TryOpenMap(viewModel.MapIds.First(id => id != viewModel.CurrentMap!.Map.Id));

        Assert.True(raised > 0);
        Assert.True(viewModel.GoBackToMapCommand.CanExecute(null));
    }

    [Fact]
    public void UndoCommand_DatabaseShownWithOnlyAMapStep_IsGreyed()
    {
        var viewModel = _content.Open(_content.TestWorld);
        EditorTestContent.PaintTopLeft(viewModel);

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.False(viewModel.History!.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void Undo_DatabaseShownAfterMapStroke_UndoesTheCatalogStep()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.Content!.SetShops([.. viewModel.Catalog!.Shops.Values, _armory]);
        var painted = EditorTestContent.PaintTopLeft(viewModel);
        viewModel.ShowDatabaseCommand.Execute(null);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.False(viewModel.Catalog.Shops.ContainsKey("armory"));
        Assert.Equal(painted, TopLeftGroundTile(viewModel));
    }

    [Fact]
    public void Undo_WorldShownAgain_ReachesTheMapStep()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var before = TopLeftGroundTile(viewModel);
        viewModel.Content!.SetShops([.. viewModel.Catalog!.Shops.Values, _armory]);
        EditorTestContent.PaintTopLeft(viewModel);
        viewModel.ShowDatabaseCommand.Execute(null);
        viewModel.ShowWorldCommand.Execute(null);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.Equal(before, TopLeftGroundTile(viewModel));
        Assert.True(viewModel.Catalog.Shops.ContainsKey("armory"));
    }

    // The stroke is still being dragged when Database is shown; a catalog step recorded after it is the newer one.
    [Fact]
    public void ShowDatabase_StrokeBeingDragged_RecordsItBeforeLaterCatalogSteps()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.Layer.Z = viewModel.CurrentMap!.Map.Layout.Layers[0].Z;
        viewModel.Painting!.StartErasing(new GridCell(0, 0));
        viewModel.ShowDatabaseCommand.Execute(null);
        viewModel.Content!.SetShops([.. viewModel.Catalog!.Shops.Values, _armory]);
        viewModel.ShowWorldCommand.Execute(null);

        viewModel.History!.UndoCommand.Execute(null);

        Assert.False(viewModel.Catalog.Shops.ContainsKey("armory"));
        Assert.Equal(TileLayer.EmptyTileId, TopLeftGroundTile(viewModel));
    }

    [Fact]
    public void ToggleCollisionCommand_DatabaseShown_IsGreyed()
    {
        var viewModel = _content.Open(_content.TestWorld);

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.False(viewModel.ToggleCollisionCommand.CanExecute(null));
    }

    [Fact]
    public void ToggleCollisionCommand_WorldShownAgain_IsLit()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.ShowDatabaseCommand.Execute(null);

        viewModel.ShowWorldCommand.Execute(null);

        Assert.True(viewModel.ToggleCollisionCommand.CanExecute(null));
    }

    [Fact]
    public void ShowDatabase_FromWorld_RaisesToggleCollisionCanExecuteChanged()
    {
        var viewModel = _content.Open(_content.TestWorld);
        var raised = 0;
        viewModel.ToggleCollisionCommand.CanExecuteChanged += (_, _) => raised++;

        viewModel.ShowDatabaseCommand.Execute(null);

        Assert.True(raised > 0);
    }

    private static int TopLeftGroundTile(MainViewModel viewModel) =>
        viewModel.CurrentMap!.Map.Layout.Layers[0].TileIdAt(new GridCell(0, 0));

    private MainViewModel OpenWithFirstObjectSelected()
    {
        var viewModel = _content.Open(_content.TestWorld);
        viewModel.Placing!.Select([viewModel.CurrentMap!.Map.Layout.Objects[0].Uid]);
        return viewModel;
    }
}
