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
    public void PlacedObjectUids_Next_LowerNumberFree_ReturnsHighestPlusOne()
    {
        Assert.Equal("chest_4", PlacedObjectUids.Next("chest", new HashSet<string> { "chest_1", "chest_3" }));
    }

    [Fact]
    public void PlacedObjectUids_Next_OtherObjectsSharingThePrefix_AreIgnored()
    {
        // chest_open is its own object, whose uids also start with "chest_"; only a bare number after the prefix counts.
        Assert.Equal("chest_2", PlacedObjectUids.Next("chest", new HashSet<string> { "chest_1", "chest_open_7", "chests_9" }));
    }

    [Fact]
    public void PlaceObjectEdit_Revert_RemovesTheObject()
    {
        var edit = new PlaceObjectEdit(Placed("tree_2", "tree", 5, 5));
        edit.Apply(_draft);

        edit.Revert(_draft);

        Assert.Equal([_tree, _chest, _farChest], _draft.Objects);
    }

    // Free positions moved by an offset: the revert puts back the exact doubles.
    [Fact]
    public void MoveObjectsEdit_ApplyThenRevert_RestoresTheExactPositions()
    {
        var draft = MapDraft.From(new MapDefinition(Layout() with { Objects = [Placed("tree_1", "tree", 0.1, 0.2), _chest] }, new MapSettings { Id = "glade", DisplayName = "Glade" }));
        var edit = new MoveObjectsEdit([new MoveObjectEdit("tree_1", new Point2D(0.1, 0.2), new Point2D(0.3, 0.4)), new MoveObjectEdit("chest_1", new Point2D(2, 1.5), new Point2D(2.2, 1.7))]);
        edit.Apply(draft);

        edit.Revert(draft);

        Assert.Equal([new Point2D(0.1, 0.2), new Point2D(2, 1.5)], draft.Objects.Select(placed => placed.Position));
    }

    // The same object moved twice in one step: the second move must be undone first to land on the first's start.
    [Fact]
    public void MoveObjectsEdit_SameObjectTwice_RevertsTheLastMoveFirst()
    {
        var edit = new MoveObjectsEdit([new MoveObjectEdit("chest_1", new Point2D(2, 1.5), new Point2D(3, 1.5)), new MoveObjectEdit("chest_1", new Point2D(3, 1.5), new Point2D(4, 1.5))]);
        edit.Apply(_draft);

        edit.Revert(_draft);

        Assert.Equal(new Point2D(2, 1.5), _draft.Objects[1].Position);
    }

    [Fact]
    public void PlaceObjectsEdit_ApplyThenRevert_LeavesTheMapAsBefore()
    {
        var edit = new PlaceObjectsEdit([Placed("chest_3", "chest", 4, 4), Placed("tree_2", "tree", 6, 6)]);
        edit.Apply(_draft);
        Assert.Equal(["chest_3", "tree_2"], _draft.Objects.Skip(3).Select(placed => placed.Uid));

        edit.Revert(_draft);

        Assert.Equal([_tree, _chest, _farChest], _draft.Objects);
    }

    [Fact]
    public void ReplacePlacedObjectEdit_ApplyThenRevert_KeepsItsPlaceInTheOrder()
    {
        var edit = new ReplacePlacedObjectEdit(_chest, _chest with { Position = new Point2D(7, 7), Z = 3 });
        edit.Apply(_draft);
        Assert.Equal((new Point2D(7, 7), 3), (_draft.Objects[1].Position, _draft.Objects[1].Z));

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
    public void PlacedObjectHits_AllAt_OverlappingObjects_ListsTheOneDrawnLastFirst()
    {
        // The tree's base (row 3) is lower on screen than the chest's (2.5), so the tree is drawn over it.
        Assert.Equal([_tree, _chest], PlacedObjectHits.AllAt(Layout(), new Point2D(2.5, 2.2), _definitions));
    }

    [Fact]
    public void PlacedObjectHits_AllAt_FieldSpriteObject_IsHitAnywhereOnItsCell()
    {
        var villager = Placed("villager_1", "villager", 6, 6);
        var npc = Definition("villager", 1, 1, []) with { FieldSpriteFile = "npc_placeholder.png" };
        var definitions = new Dictionary<string, ObjectDefinition>(_definitions) { ["villager"] = npc };
        var map = Layout() with { Objects = [villager] };

        Assert.Equal([villager], PlacedObjectHits.AllAt(map, new Point2D(6.1, 6.2), definitions));
        Assert.Empty(PlacedObjectHits.AllAt(map, new Point2D(7.0, 6.5), definitions));
    }

    [Fact]
    public void PlacedObjectHits_AllAt_CellWithoutTheObjectsTile_MissesIt()
    {
        Assert.Empty(PlacedObjectHits.AllAt(Layout(), new Point2D(2.5, 1.2), _definitions));
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
