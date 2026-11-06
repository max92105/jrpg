using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Maps.Editing;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// Ctrl + click selection, and the selection moved by the arrow keys or a Shift + drag (decisions 509, 510), on the
/// forest without its own objects and two chests at (2, 2): "front" on layer 2, listed first, and "back" on layer 1,
/// listed last, so the list order alone would pick the wrong one.
/// </summary>
public sealed class ObjectPlacingMoveTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly ObjectPlacingViewModel _placing;

    public ObjectPlacingMoveTests()
    {
        var forest = _catalog.Maps["forest"];
        PlacedObject[] chests =
        [
            new() { Uid = "front", ObjectId = "chest", Position = new Point2D(2, 2), Z = 2 },
            new() { Uid = "back", ObjectId = "chest", Position = new Point2D(2, 2), Z = 1 },
        ];
        var map = new MapDefinition(forest.Layout with { Objects = chests }, forest.Settings);
        _placing = new ObjectPlacingViewModel(new CatalogDraft(_catalog), new LayerChoiceViewModel(), new OpenMaps()) { OpenMap = new MapViewModel(map) };
    }

    private MapViewModel Map => _placing.OpenMap!;

    [Fact]
    public void SelectObjectAt_TwoObjectsOverlapping_SelectsOnlyTheTopOne()
    {
        _placing.SelectObjectAt(new Point2D(2.5, 2.5));

        Assert.Equal(["front"], _placing.SelectedObjectUids);
    }

    [Fact]
    public void SelectObjectAt_EmptyGround_ClearsTheSelection()
    {
        _placing.SelectObjectAt(new Point2D(2.5, 2.5));

        _placing.SelectObjectAt(new Point2D(6.5, 6.5));

        Assert.Empty(_placing.SelectedObjectUids);
    }

    [Fact]
    public void ClearSelection_TwoSelected_EmptiesIt()
    {
        SelectBoth();

        _placing.ClearSelection();

        Assert.Empty(_placing.SelectedObjectUids);
    }

    [Fact]
    public void MoveSelectedObjects_ArrowStep_MovesBothOneCell()
    {
        SelectBoth();

        _placing.MoveSelectedObjects(ObjectPlacingViewModel.NudgeCells, 0);

        Assert.All(Map.Map.Layout.Objects, placed => Assert.Equal(new Point2D(3, 2), placed.Position));
    }

    [Fact]
    public void MoveSelectedObjects_FineStep_MovesByOneSixteenth()
    {
        SelectBoth();

        _placing.MoveSelectedObjects(0, -ObjectPlacingViewModel.FineNudgeCells);

        Assert.All(Map.Map.Layout.Objects, placed => Assert.Equal(new Point2D(2, 2 - 0.0625), placed.Position));
    }

    // Front from (0.1, 0.3), where 0.1 + 1 - 1 isn't 0.1 in doubles: an undo taking the offset off would miss it.
    [Fact]
    public void MoveSelectedObjects_Undo_PutsBothBackExactlyInOneStep()
    {
        Map.Do(new MoveObjectEdit("front", new Point2D(2, 2), new Point2D(0.1, 0.3)));
        _placing.SelectObjectsIn(new Point2D(0, 0), new Point2D(4, 4));
        _placing.MoveSelectedObjects(ObjectPlacingViewModel.NudgeCells, 0);

        Map.UndoCommand.Execute(null);

        Assert.Equal([new Point2D(0.1, 0.3), new Point2D(2, 2)], Map.Map.Layout.Objects.Select(placed => placed.Position));
        Assert.True(Map.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void MoveSelectedObjects_NothingSelected_RecordsNoStep()
    {
        _placing.MoveSelectedObjects(ObjectPlacingViewModel.NudgeCells, 0);

        Assert.False(Map.IsDirty);
    }

    // A third chest placed, selected and undone: the selection drops it; the nudge moves the other two.
    [Fact]
    public void MoveSelectedObjects_SelectedObjectUndoneAway_MovesOnlyTheOthers()
    {
        _placing.Browser.SelectedObject = _catalog.Objects["chest"];
        _placing.PlaceObjectAt(new Point2D(8.5, 8.5));
        var placedUid = Map.Map.Layout.Objects[^1].Uid;
        _placing.SelectObjectsIn(new Point2D(0, 0), new Point2D(10, 10));
        Map.UndoCommand.Execute(null);

        _placing.MoveSelectedObjects(ObjectPlacingViewModel.NudgeCells, 0);

        Assert.DoesNotContain(placedUid, _placing.SelectedObjectUids);
        Assert.All(Map.Map.Layout.Objects, placed => Assert.Equal(new Point2D(3, 2), placed.Position));
    }

    [Fact]
    public void MoveSelectedObjects_TwoSelected_MovesBothByTheOffset()
    {
        SelectBoth();

        _placing.MoveSelectedObjects(1.5, 2.25);

        Assert.All(Map.Map.Layout.Objects, placed => Assert.Equal(new Point2D(3.5, 4.25), placed.Position));
    }

    [Fact]
    public void MoveSelectedObjects_ZeroOffset_RecordsNoStep()
    {
        SelectBoth();

        _placing.MoveSelectedObjects(0, 0);

        Assert.False(Map.IsDirty);
    }

    [Fact]
    public void MoveSelectedObjects_AnUnselectedObject_StaysPut()
    {
        _placing.SelectObjectAt(new Point2D(2.5, 2.5));

        _placing.MoveSelectedObjects(1, 0);

        Assert.Equal([new Point2D(3, 2), new Point2D(2, 2)], Map.Map.Layout.Objects.Select(placed => placed.Position));
    }

    [Fact]
    public void IsSelected_SelectedObject_IsTrue()
    {
        _placing.SelectObjectAt(new Point2D(2.5, 2.5));

        Assert.True(_placing.IsSelected("front"));
    }

    [Fact]
    public void IsSelected_UnselectedObject_IsFalse()
    {
        _placing.SelectObjectAt(new Point2D(2.5, 2.5));

        Assert.False(_placing.IsSelected("back"));
    }

    private void SelectBoth() => _placing.SelectObjectsIn(new Point2D(1.5, 1.5), new Point2D(3.5, 3.5));
}
