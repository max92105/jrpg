using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// Ctrl + click's pick cycling through the objects under a point, the panel's note and Next under, and the object Shift
/// + drag grabs (decisions 523, 524): three chests placed on one spot of the forest map, cleared of its objects.
/// </summary>
public sealed class ObjectPlacingSelectionTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly Point2D _stack = new(4.5, 6.5);

    private readonly ObjectPlacingViewModel _placing;

    public ObjectPlacingSelectionTests()
    {
        var forest = _catalog.Maps["forest"];
        var emptyForest = new MapDefinition(forest.Layout with { Objects = [] }, forest.Settings);
        _placing = new ObjectPlacingViewModel(new CatalogDraft(_catalog), new LayerChoiceViewModel(), new OpenMaps()) { OpenMap = new MapViewModel(emptyForest) };
        _placing.Browser.SelectedObject = _catalog.Objects["chest"];
    }

    [Fact]
    public void SelectObjectAt_AgainOnTheSameSpot_SelectsTheNextUnder()
    {
        PlaceChests(3);
        _placing.SelectObjectAt(_stack);
        Assert.Equal(["chest_3"], _placing.SelectedObjectUids);

        _placing.SelectObjectAt(_stack);

        Assert.Equal(["chest_2"], _placing.SelectedObjectUids);
    }

    [Fact]
    public void SelectObjectAt_AfterTheLast_WrapsToTheTop()
    {
        PlaceChests(3);
        Pick(3);
        Assert.Equal(["chest_1"], _placing.SelectedObjectUids);

        _placing.SelectObjectAt(_stack);

        Assert.Equal(["chest_3"], _placing.SelectedObjectUids);
    }

    [Fact]
    public void SelectObjectAt_SelectedObjectNotUnderThePoint_SelectsTheTop()
    {
        PlaceChests(3);
        _placing.PlaceObjectAt(new Point2D(9.5, 9.5));
        _placing.Select(["chest_4"]);

        _placing.SelectObjectAt(_stack);

        Assert.Equal(["chest_3"], _placing.SelectedObjectUids);
    }

    [Fact]
    public void PickNote_ThreeObjectsHere_SaysTheirCountAndPlace()
    {
        PlaceChests(3);

        Pick(2);

        Assert.Equal("chest_2 selected: 2 of 3 objects here", _placing.Picking.Note);
    }

    [Fact]
    public void PickNote_OneObjectHere_IsEmpty()
    {
        PlaceChests(1);

        _placing.SelectObjectAt(_stack);

        Assert.Equal(["chest_1"], _placing.SelectedObjectUids);
        Assert.Equal("", _placing.Picking.Note);
    }

    [Fact]
    public void PickNote_SelectionChangedElsewhere_IsCleared()
    {
        PlaceChests(3);
        _placing.SelectObjectAt(_stack);

        _placing.ClearSelection();

        Assert.Equal("", _placing.Picking.Note);
        Assert.False(_placing.Picking.SelectNextUnderCommand.CanExecute(null));
    }

    // Moving the picked object keeps it selected, but what lies at the pick's point is no longer what the note says.
    [Fact]
    public void PickNote_PickedObjectMoved_IsCleared()
    {
        PlaceChests(3);
        _placing.SelectObjectAt(_stack);

        _placing.MoveObject("chest_3", new Point2D(0, 0));

        Assert.Equal("", _placing.Picking.Note);
        Assert.False(_placing.Picking.SelectNextUnderCommand.CanExecute(null));
    }

    [Fact]
    public void SelectNextUnderCommand_OneObjectAtTheLastPick_CannotExecute()
    {
        PlaceChests(1);

        _placing.SelectObjectAt(_stack);

        Assert.False(_placing.Picking.SelectNextUnderCommand.CanExecute(null));
    }

    [Fact]
    public void SelectNextUnderCommand_TwoObjectsAtTheLastPick_CanExecute()
    {
        PlaceChests(2);
        var raised = 0;
        _placing.Picking.SelectNextUnderCommand.CanExecuteChanged += (_, _) => raised++;

        _placing.SelectObjectAt(_stack);

        Assert.True(_placing.Picking.SelectNextUnderCommand.CanExecute(null));
        Assert.Equal(1, raised);
    }

    [Fact]
    public void SelectNextUnderCommand_Executed_SelectsTheNextUnderTheLastPick()
    {
        PlaceChests(3);
        _placing.SelectObjectAt(_stack);

        _placing.Picking.SelectNextUnderCommand.Execute(null);

        Assert.Equal(["chest_2"], _placing.SelectedObjectUids);
        Assert.Equal("chest_2 selected: 2 of 3 objects here", _placing.Picking.Note);
    }

    [Fact]
    public void GrabbedObjectAt_SelectedObjectCovered_GivesTheSelected()
    {
        PlaceChests(3);
        _placing.Select(["chest_1"]);

        Assert.Equal("chest_1", _placing.GrabbedObjectAt(_stack)?.Uid);
    }

    [Fact]
    public void GrabbedObjectAt_NothingSelectedThere_GivesTheTop()
    {
        PlaceChests(3);

        Assert.Equal("chest_3", _placing.GrabbedObjectAt(_stack)?.Uid);
    }

    // chest_1 first, so chest_3 is drawn on top: same base, placed last.
    private void PlaceChests(int count)
    {
        for (var placed = 0; placed < count; placed++)
        {
            _placing.PlaceObjectAt(_stack);
        }
    }

    private void Pick(int times)
    {
        for (var pick = 0; pick < times; pick++)
        {
            _placing.SelectObjectAt(_stack);
        }
    }
}
