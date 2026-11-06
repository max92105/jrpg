using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// Snapping to the grid (decision 511), on the forest without its own objects and two chests at free places: "near" at
/// (2.3, 2.3) and "far" at (7.25, 3.5); a 2 x 2 chest chosen to place.
/// </summary>
public sealed class ObjectPlacingSnapTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly ObjectPlacingViewModel _placing;

    public ObjectPlacingSnapTests()
    {
        var forest = _catalog.Maps["forest"];
        PlacedObject[] chests =
        [
            new() { Uid = "near", ObjectId = "chest", Position = new Point2D(2.3, 2.3), Z = 1 },
            new() { Uid = "far", ObjectId = "chest", Position = new Point2D(7.25, 3.5), Z = 1 },
        ];
        var map = new MapDefinition(forest.Layout with { Objects = chests }, forest.Settings);
        _placing = new ObjectPlacingViewModel(new CatalogDraft(_catalog), new LayerChoiceViewModel(), new OpenMaps()) { OpenMap = new MapViewModel(map) };
        _placing.Browser.SelectedObject = _catalog.Objects["chest"] with { Width = 2, Height = 2 };
    }

    private MapViewModel Map => _placing.OpenMap!;

    [Fact]
    public void IsGridSnapOn_AtTheStart_IsOff()
    {
        Assert.False(_placing.IsGridSnapOn);
    }

    [Fact]
    public void ToggleGridSnap_Twice_TurnsItOnThenOffAndRaisesIt()
    {
        var raised = 0;
        _placing.PropertyChanged += (_, e) => raised += e.PropertyName == nameof(ObjectPlacingViewModel.IsGridSnapOn) ? 1 : 0;

        _placing.ToggleGridSnap();
        var afterOne = _placing.IsGridSnapOn;
        _placing.ToggleGridSnap();

        Assert.True(afterOne);
        Assert.False(_placing.IsGridSnapOn);
        Assert.Equal(2, raised);
    }

    // The 2 x 2 chest centered on (5.3, 6.8) has its top-left at (4.3, 5.8), the nearest whole cell (4, 6).
    [Fact]
    public void PlaceObjectOnGridAt_FreeCenter_PlacesOnWholeCells()
    {
        _placing.PlaceObjectOnGridAt(new Point2D(5.3, 6.8));

        Assert.Equal(new Point2D(4, 6), Map.Map.Layout.Objects[^1].Position);
    }

    [Fact]
    public void PlaceObjectOnGridAt_CenterOffTheMap_PlacesNothing()
    {
        _placing.PlaceObjectOnGridAt(new Point2D(-1, -1));

        Assert.False(Map.IsDirty);
    }

    [Fact]
    public void MoveObjectOnGrid_FreeTarget_LandsOnWholeCells()
    {
        _placing.MoveObjectOnGrid("near", new Point2D(3.6, 1.2));

        Assert.Equal(new Point2D(4, 1), Map.Map.Layout.Objects[0].Position);
    }

    // Near grabbed and left at (5.4, 4.2) lands on (5, 4); far moves by the same offset and stays free.
    [Fact]
    public void MoveSelectedObjectsOnGrid_GrabbedLandsOnTheGrid_OthersKeepTheirOffsets()
    {
        SelectBoth();

        _placing.MoveSelectedObjectsOnGrid("near", new Point2D(5.4, 4.2));

        Assert.Equal(new Point2D(5, 4), Map.Map.Layout.Objects[0].Position);
        Assert.Equal(new Point2D(7.25 + (5 - 2.3), 3.5 + (4 - 2.3)), Map.Map.Layout.Objects[1].Position);
    }

    [Fact]
    public void MoveSelectedObjectsOnGrid_Undo_PutsAllBackInOneStep()
    {
        SelectBoth();
        _placing.MoveSelectedObjectsOnGrid("near", new Point2D(5.4, 4.2));

        Map.UndoCommand.Execute(null);

        Assert.Equal([new Point2D(2.3, 2.3), new Point2D(7.25, 3.5)], Map.Map.Layout.Objects.Select(placed => placed.Position));
        Assert.False(Map.UndoCommand.CanExecute(null));
    }

    private void SelectBoth() => _placing.SelectObjectsIn(new Point2D(0, 0), new Point2D(10, 10));
}
