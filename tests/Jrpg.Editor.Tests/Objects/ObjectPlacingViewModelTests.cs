using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Maps;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

public sealed class ObjectPlacingViewModelTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    private readonly LayerChoiceViewModel _layer = new();
    private readonly CatalogDraft _draft = new(_catalog);
    private readonly ObjectPlacingViewModel _placing;

    // The forest map open, the chest chosen to place.
    public ObjectPlacingViewModelTests()
    {
        _placing = new ObjectPlacingViewModel(_draft, _layer) { OpenMap = new MapViewModel(_catalog.Maps["forest"]) };
        _placing.Browser.SelectedObject = _catalog.Objects["chest"];
    }

    private MapViewModel Map => _placing.OpenMap!;

    [Fact]
    public void StartEditingShapes_NothingChosenOrSelected_ReturnsNull()
    {
        _placing.Browser.SelectedObject = null;

        Assert.Null(_placing.StartEditingShapes());
        Assert.False(_placing.CanEditShapes);
    }

    // A tree alone on the forest map, at (2, 2), while the panel has the chest chosen.
    [Fact]
    public void StartEditingShapes_OnePlacedObjectSelected_EditsItsDefinition()
    {
        var tree = OpenForestWithOnlyATree();
        _placing.SelectObjectsIn(TileCenterOf(tree), TileCenterOf(tree));

        var shapes = _placing.StartEditingShapes();

        Assert.Equal("tree", shapes?.ObjectId);
        Assert.True(_placing.CanEditShapes);
    }

    [Fact]
    public void StartEditingShapesAt_PlacedObjectUnderPoint_EditsItsDefinition()
    {
        var tree = OpenForestWithOnlyATree();

        Assert.Equal("tree", _placing.StartEditingShapesAt(TileCenterOf(tree))?.ObjectId);
    }

    [Fact]
    public void StartEditingShapesAt_EmptyCell_ReturnsNull()
    {
        OpenForestWithOnlyATree();

        Assert.Null(_placing.StartEditingShapesAt(new Point2D(0.5, 0.5)));
    }

    [Fact]
    public void ApplyShapes_ChangedOutline_PutsTheObjectInTheCatalog()
    {
        var shapes = _placing.StartEditingShapes()!;
        shapes.ClearShapeCommand.Execute(null);

        _placing.ApplyShapes(shapes);

        Assert.Empty(_draft.Current.Objects["chest"].CollisionPolygon);
        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void PlaceObjectAt_ObjectChosen_CentersItOnThePointWithTheNextUid()
    {
        _layer.Z = 1;

        _placing.PlaceObjectAt(new Point2D(4.5, 6.5));
        _placing.PlaceObjectAt(new Point2D(8, 8));

        var placed = Map.Map.Layout.Objects;
        Assert.Equal(("chest_1", new Point2D(4, 6), 1), (placed[^2].Uid, placed[^2].Position, placed[^2].Z));
        Assert.Equal("chest_2", placed[^1].Uid);
    }

    [Fact]
    public void PlaceObjectAt_NoObjectChosen_ChangesNothing()
    {
        _placing.Browser.SelectedObject = null;

        _placing.PlaceObjectAt(new Point2D(4.5, 6.5));

        Assert.False(Map.IsDirty);
    }

    [Fact]
    public void PlaceObjectAt_PointOffTheMap_PlacesNothing()
    {
        _placing.PlaceObjectAt(new Point2D(-0.5, 3));

        Assert.False(Map.IsDirty);
    }

    [Fact]
    public void MoveObject_ThenUndo_PutsItBack()
    {
        _placing.PlaceObjectAt(new Point2D(4.5, 6.5));

        _placing.MoveObject("chest_1", new Point2D(10, 3));
        Assert.Equal(new Point2D(10, 3), _placing.ObjectAt(new Point2D(10.5, 3.5))?.Position);
        Map.UndoCommand.Execute(null);

        Assert.Equal(new Point2D(4, 6), _placing.ObjectAt(new Point2D(4.5, 6.5))?.Position);
    }

    [Fact]
    public void DeleteSelectedObjects_RemovesThemAsOneStepAndClearsTheSelection()
    {
        _placing.PlaceObjectAt(new Point2D(4.5, 6.5));
        _placing.PlaceObjectAt(new Point2D(5.5, 6.5));
        var before = Map.Map.Layout.Objects.Count;
        _placing.SelectObjectsIn(new Point2D(3.9, 5.9), new Point2D(6.1, 7.1));
        Assert.Equal(["chest_1", "chest_2"], _placing.SelectedObjectUids);

        _placing.DeleteSelectedObjects();

        Assert.Equal(before - 2, Map.Map.Layout.Objects.Count);
        Assert.Empty(_placing.SelectedObjectUids);
        Map.UndoCommand.Execute(null);
        Assert.Equal(before, Map.Map.Layout.Objects.Count);
    }

    [Fact]
    public void DeleteSelectedObjectsCommand_NothingSelected_ChangesNothing()
    {
        _placing.DeleteSelectedObjectsCommand.Execute(null);

        Assert.False(Map.IsDirty);
    }

    [Fact]
    public void RemoveObject_IsUndoable()
    {
        _placing.PlaceObjectAt(new Point2D(4.5, 6.5));

        _placing.RemoveObject("chest_1");

        Assert.Null(_placing.ObjectAt(new Point2D(4.5, 6.5)));
        Map.UndoCommand.Execute(null);
        Assert.Equal("chest_1", _placing.ObjectAt(new Point2D(4.5, 6.5))?.Uid);
    }

    [Fact]
    public void SelectedObjectUids_AfterUndoingThePlacement_DropsTheObject()
    {
        _placing.PlaceObjectAt(new Point2D(4.5, 6.5));
        _placing.SelectObjectsIn(new Point2D(3.9, 5.9), new Point2D(5.1, 7.1));

        Map.UndoCommand.Execute(null);
        _placing.PlaceObjectAt(new Point2D(9.5, 9.5));

        Assert.Empty(_placing.SelectedObjectUids);
    }

    [Fact]
    public void OpenMap_AnotherMap_ClearsTheSelection()
    {
        _placing.PlaceObjectAt(new Point2D(4.5, 6.5));
        _placing.SelectObjectsIn(new Point2D(3.9, 5.9), new Point2D(5.1, 7.1));

        _placing.OpenMap = new MapViewModel(_catalog.Maps["start"]);

        Assert.Empty(_placing.SelectedObjectUids);
    }

    private PlacedObject OpenForestWithOnlyATree()
    {
        var forest = _catalog.Maps["forest"];
        var tree = new PlacedObject { Uid = "tree_1", ObjectId = "tree", Position = new Point2D(2, 2), Z = 1 };
        _placing.OpenMap = new MapViewModel(new MapDefinition(forest.Layout with { Objects = [tree] }, forest.Settings));
        return tree;
    }

    // The middle of the object's first tile, which the object covers.
    private static Point2D TileCenterOf(PlacedObject placed)
    {
        var tile = _catalog.Objects[placed.ObjectId].Tiles[0];
        return new Point2D(placed.Position.X + tile.X + 0.5, placed.Position.Y + tile.Y + 0.5);
    }
}
