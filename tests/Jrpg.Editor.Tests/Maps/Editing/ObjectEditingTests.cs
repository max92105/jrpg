using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class ObjectEditingTests
{
    // A 2x2 tree whose top-right cell has no tile, and a 1x1 chest.
    private static readonly Dictionary<string, ObjectDefinition> _definitions = new()
    {
        ["tree"] = Definition("tree", 2, 2, [new ObjectTile(0, 0, 5), new ObjectTile(0, 1, 6), new ObjectTile(1, 1, 7)]),
        ["chest"] = Definition("chest", 1, 1, [new ObjectTile(0, 0, 8)]),
    };

    private static readonly PlacedObject _tree = Placed("tree_1", "tree", 1, 1);
    private static readonly PlacedObject _chest = Placed("chest_1", "chest", 2, 1.5);
    private static readonly PlacedObject _farChest = Placed("chest_2", "chest", 8, 8);

    private readonly MapDraft _draft = MapDraft.From(new MapDefinition(
        new MapLayout { Id = "glade", Width = 10, Height = 10, Layers = [], Objects = [_tree, _chest, _farChest] },
        new MapSettings { Id = "glade", DisplayName = "Glade" }));

    [Fact]
    public void PlacedObjectUids_Next_Chest1Taken_ReturnsChest2()
    {
        Assert.Equal("chest_2", PlacedObjectUids.Next("chest", new HashSet<string> { "chest_1", "2@3.682,26.885" }));
    }

    [Fact]
    public void PlacedObjectUids_Next_NoneTaken_ReturnsSuffixOne()
    {
        Assert.Equal("chest_1", PlacedObjectUids.Next("chest", new HashSet<string>()));
    }

    [Fact]
    public void PlacedObjectUids_Next_GapLeftByARemovedObject_ReusesIt()
    {
        Assert.Equal("chest_2", PlacedObjectUids.Next("chest", new HashSet<string> { "chest_1", "chest_3" }));
    }

    [Fact]
    public void PlaceObjectEdit_Revert_RemovesTheObject()
    {
        var edit = new PlaceObjectEdit(Placed("tree_2", "tree", 5, 5));
        edit.Apply(_draft);

        edit.Revert(_draft);

        Assert.Equal([_tree, _chest, _farChest], _draft.Objects);
    }

    [Fact]
    public void MoveObjectEdit_Revert_RestoresPosition()
    {
        var edit = new MoveObjectEdit("chest_1", new Point2D(2, 1.5), new Point2D(4.25, 3));
        edit.Apply(_draft);
        Assert.Equal(new Point2D(4.25, 3), _draft.Objects[1].Position);

        edit.Revert(_draft);

        Assert.Equal([_tree, _chest, _farChest], _draft.Objects);
    }

    [Fact]
    public void RemoveObjectsEdit_Revert_RestoresObjectsInPlacementOrder()
    {
        var edit = new RemoveObjectsEdit(_draft.Objects, ["tree_1", "chest_2"]);
        edit.Apply(_draft);
        Assert.Equal([_chest], _draft.Objects);

        edit.Revert(_draft);

        Assert.Equal([_tree, _chest, _farChest], _draft.Objects);
    }

    [Fact]
    public void MapDraft_ToDefinition_AfterPlacing_IncludesTheObject()
    {
        var placed = Placed("tree_2", "tree", 5, 5);

        new PlaceObjectEdit(placed).Apply(_draft);

        Assert.Equal(placed, _draft.ToDefinition().Layout.Objects[^1]);
    }

    [Fact]
    public void PlacedObjectHits_InRectangle_ReturnsOnlyOverlapping()
    {
        // Overlaps the tree's bottom-right cell and the chest; the far chest is outside.
        var hits = PlacedObjectHits.InRectangle(Layout(), new Point2D(3.5, 1.8), new Point2D(2.5, 2.2), _definitions);

        Assert.Equal([_tree, _chest], hits);
    }

    [Fact]
    public void PlacedObjectHits_InRectangle_OnlyTouchingAnEdge_LeavesItOut()
    {
        Assert.Empty(PlacedObjectHits.InRectangle(Layout(), new Point2D(3, 3), new Point2D(4, 4), _definitions));
    }

    [Fact]
    public void PlacedObjectHits_TopAt_OverlappingObjects_ReturnsTheOneDrawnLast()
    {
        // The tree's base (row 3) is lower on screen than the chest's (2.5), so the tree is drawn over it.
        Assert.Equal(_tree, PlacedObjectHits.TopAt(Layout(), new Point2D(2.5, 2.2), _definitions));
    }

    [Fact]
    public void PlacedObjectHits_TopAt_CellWithoutTheObjectsTile_MissesIt()
    {
        Assert.Null(PlacedObjectHits.TopAt(Layout(), new Point2D(2.5, 1.2), _definitions));
    }

    private static MapLayout Layout() => new() { Id = "glade", Width = 10, Height = 10, Layers = [], Objects = [_tree, _chest, _farChest] };

    private static PlacedObject Placed(string uid, string objectId, double x, double y) =>
        new() { Uid = uid, ObjectId = objectId, Position = new Point2D(x, y), Z = 1 };

    private static ObjectDefinition Definition(string id, int width, int height, IReadOnlyList<ObjectTile> tiles) => new()
    {
        Id = id,
        Width = width,
        Height = height,
        CollisionPolygon = [],
        Category = "General",
        Tiles = tiles,
        AbovePlayerZones = [],
    };
}
