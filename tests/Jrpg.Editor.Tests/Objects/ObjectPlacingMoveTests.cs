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
/// Ctrl + click selection and the arrow keys' nudges (decisions 509, 510), on the forest without its own objects and
/// two chests at (2, 2): "front" on layer 2, listed first, and "back" on layer 1, listed last, so the list order alone
/// would pick the wrong one.
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
    public void NudgeSelectedObjects_TwoSelected_MovesBothByTheStep()
    {
        SelectBoth();

        _placing.NudgeSelectedObjects(ObjectPlacingViewModel.NudgeCells, 0);

        Assert.All(Map.Map.Layout.Objects, placed => Assert.Equal(new Point2D(3, 2), placed.Position));
    }

    [Fact]
    public void NudgeSelectedObjects_FineStep_MovesByOneSixteenth()
    {
        SelectBoth();

        _placing.NudgeSelectedObjects(0, -ObjectPlacingViewModel.FineNudgeCells);

        Assert.All(Map.Map.Layout.Objects, placed => Assert.Equal(new Point2D(2, 2 - 0.0625), placed.Position));
    }

    // Front from (0.1, 0.3), where 0.1 + 1 - 1 isn't 0.1 in doubles: an undo taking the offset off would miss it.
    [Fact]
    public void NudgeSelectedObjects_Undo_PutsBothBackExactlyInOneStep()
    {
        Map.Do(new MoveObjectEdit("front", new Point2D(2, 2), new Point2D(0.1, 0.3)));
        _placing.SelectObjectsIn(new Point2D(0, 0), new Point2D(4, 4));
        _placing.NudgeSelectedObjects(ObjectPlacingViewModel.NudgeCells, 0);

        Map.UndoCommand.Execute(null);

        Assert.Equal([new Point2D(0.1, 0.3), new Point2D(2, 2)], Map.Map.Layout.Objects.Select(placed => placed.Position));
        Assert.True(Map.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void NudgeSelectedObjects_NothingSelected_RecordsNoStep()
    {
        _placing.NudgeSelectedObjects(ObjectPlacingViewModel.NudgeCells, 0);

        Assert.False(Map.IsDirty);
    }

    // A third chest placed, selected and undone: the selection drops it; the nudge moves the other two.
    [Fact]
    public void NudgeSelectedObjects_SelectedObjectUndoneAway_MovesOnlyTheOthers()
    {
        _placing.Browser.SelectedObject = _catalog.Objects["chest"];
        _placing.PlaceObjectAt(new Point2D(8.5, 8.5));
        var placedUid = Map.Map.Layout.Objects[^1].Uid;
        _placing.SelectObjectsIn(new Point2D(0, 0), new Point2D(10, 10));
        Map.UndoCommand.Execute(null);

        _placing.NudgeSelectedObjects(ObjectPlacingViewModel.NudgeCells, 0);

        Assert.DoesNotContain(placedUid, _placing.SelectedObjectUids);
        Assert.All(Map.Map.Layout.Objects, placed => Assert.Equal(new Point2D(3, 2), placed.Position));
    }

    private void SelectBoth() => _placing.SelectObjectsIn(new Point2D(1.5, 1.5), new Point2D(3.5, 3.5));
}
