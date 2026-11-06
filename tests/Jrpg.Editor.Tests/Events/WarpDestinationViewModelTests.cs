using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Events;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Events;

/// <summary>
/// The warp destination picker over two maps built in code: "hall", 4 x 3 of walkable tiles, (1, 0) blocked and (3, 2)
/// empty, a crate whose outline fills (2, 0), a walk-into cell event on (0, 2) and a door whose walk-into event covers
/// (3, 0), another on (1, 2) whose event runs only for a crate; "closet", 2 x 2.
/// </summary>
public sealed class WarpDestinationViewModelTests
{
    private static readonly ContentCatalog _testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
    private static readonly WarpMaps _maps = BuildMaps();

    [Fact]
    public void New_MapAndCell_StartsOnThem()
    {
        var picker = new WarpDestinationViewModel(_maps, "hall", new GridCell(1, 1));

        Assert.Equal("hall", picker.MapId);
        Assert.Equal(new GridCell(1, 1), picker.Cell);
    }

    [Fact]
    public void New_UnknownMap_StartsWithNoMapAndAProblem()
    {
        var picker = new WarpDestinationViewModel(_maps, "nowhere", new GridCell(1, 1));

        Assert.Null(picker.MapId);
        Assert.Null(picker.Cell);
        Assert.Equal("Choose the map to warp to.", picker.Problem);
    }

    [Fact]
    public void New_CellOutsideTheMap_StartsWithNoCell()
    {
        var picker = new WarpDestinationViewModel(_maps, "closet", new GridCell(3, 2));

        Assert.Null(picker.Cell);
    }

    [Fact]
    public void ChooseCell_InsideTheMap_SetsItAndItsText()
    {
        var picker = new WarpDestinationViewModel(_maps, "hall", null);

        picker.ChooseCell(new GridCell(3, 1));

        Assert.Equal(new GridCell(3, 1), picker.Cell);
        Assert.Equal("Cell (3, 1)", picker.CellText);
    }

    [Fact]
    public void ChooseCell_PastTheRightEdge_LeavesTheCell()
    {
        var picker = new WarpDestinationViewModel(_maps, "hall", new GridCell(1, 1));

        picker.ChooseCell(new GridCell(4, 1));

        Assert.Equal(new GridCell(1, 1), picker.Cell);
    }

    [Fact]
    public void ChooseCell_AtMinusOne_LeavesTheCell()
    {
        var picker = new WarpDestinationViewModel(_maps, "hall", new GridCell(1, 1));

        picker.ChooseCell(new GridCell(-1, 1));

        Assert.Equal(new GridCell(1, 1), picker.Cell);
    }

    [Fact]
    public void MapId_ChangedToAMapTheCellFits_KeepsTheCell()
    {
        var picker = new WarpDestinationViewModel(_maps, "hall", new GridCell(1, 1));

        picker.MapId = "closet";

        Assert.Equal(new GridCell(1, 1), picker.Cell);
    }

    [Fact]
    public void MapId_ChangedToASmallerMap_ClearsTheCell()
    {
        var picker = new WarpDestinationViewModel(_maps, "hall", new GridCell(3, 1));

        picker.MapId = "closet";

        Assert.Null(picker.Cell);
        Assert.Equal("Click a cell on the map", picker.CellText);
    }

    [Fact]
    public void Warning_BlockedCell_NamesIt()
    {
        Assert.Equal("The player can't stand on (1, 0): no tile, or a blocked tile.", WarningAt(1, 0));
    }

    [Fact]
    public void Warning_CellWithNoTile_NamesIt()
    {
        Assert.Equal("The player can't stand on (3, 2): no tile, or a blocked tile.", WarningAt(3, 2));
    }

    [Fact]
    public void Warning_CellInsideAnOutline_NamesTheObject()
    {
        Assert.Equal("(2, 0) is inside the collision outline of crate_1.", WarningAt(2, 0));
    }

    [Fact]
    public void Warning_CellBesideTheOutline_IsNull()
    {
        Assert.Null(WarningAt(2, 1));
    }

    [Fact]
    public void Warning_CellWithAWalkIntoCellEvent_NamesTheEvent()
    {
        Assert.Equal("(0, 2) holds walk-into event trap: stepping off and back on starts it.", WarningAt(0, 2));
    }

    [Fact]
    public void Warning_CellUnderAnObjectWithAWalkIntoEvent_NamesTheEvent()
    {
        Assert.Equal("(3, 0) is inside an object with walk-into event door_1_0: the first step starts it.", WarningAt(3, 0));
    }

    [Fact]
    public void Warning_CellWithAPressedEventOnly_IsNull()
    {
        Assert.Null(WarningAt(0, 1));
    }

    // door_3's walk-into event runs only while the object is a crate, so arriving there starts nothing.
    [Fact]
    public void Warning_CellUnderAWalkIntoEventForAnotherObject_IsNull()
    {
        Assert.Null(WarningAt(1, 2));
    }

    [Fact]
    public void Warning_WalkableEmptyCell_IsNull()
    {
        Assert.Null(WarningAt(1, 1));
    }

    [Fact]
    public void Problem_NoCell_AsksForTheCell()
    {
        Assert.Equal("Click the cell to arrive on.", new WarpDestinationViewModel(_maps, "hall", null).Problem);
    }

    [Fact]
    public void Problem_MapAndCell_IsNull()
    {
        Assert.Null(new WarpDestinationViewModel(_maps, "hall", new GridCell(1, 1)).Problem);
    }

    [Fact]
    public void ChooseCell_RaisesCellTextWarningAndProblem()
    {
        var picker = new WarpDestinationViewModel(_maps, "hall", null);
        var changed = new List<string?>();
        picker.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        picker.ChooseCell(new GridCell(1, 1));

        Assert.Contains(nameof(WarpDestinationViewModel.CellText), changed);
        Assert.Contains(nameof(WarpDestinationViewModel.Warning), changed);
        Assert.Contains(nameof(WarpDestinationViewModel.Problem), changed);
    }

    // From the blocked (1, 0) of hall to closet, where the kept cell is walkable: the warning must be read again.
    [Fact]
    public void MapId_Changed_RaisesLayoutWarningAndProblem()
    {
        var picker = new WarpDestinationViewModel(_maps, "hall", new GridCell(1, 0));
        var changed = new List<string?>();
        picker.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        picker.MapId = "closet";

        Assert.Contains(nameof(WarpDestinationViewModel.Layout), changed);
        Assert.Contains(nameof(WarpDestinationViewModel.Warning), changed);
        Assert.Contains(nameof(WarpDestinationViewModel.Problem), changed);
        Assert.Null(picker.Warning);
    }

    private static string? WarningAt(int x, int y) => new WarpDestinationViewModel(_maps, "hall", new GridCell(x, y)).Warning;

    private static WarpMaps BuildMaps()
    {
        const int walkable = 1;
        const int blocked = 2;
        var crate = OneCellObject("crate", [new(0, 0), new(1, 0), new(1, 1), new(0, 1)]);
        var door = OneCellObject("door", []);
        var hall = new MapLayout
        {
            Id = "hall",
            Width = 4,
            Height = 3,
            Layers = [new TileLayer { Z = 0, Rows = [[walkable, blocked, walkable, walkable], [walkable, walkable, walkable, walkable], [walkable, walkable, walkable, 0]] }],
            Objects =
            [
                new PlacedObject { Uid = "crate_1", ObjectId = "crate", Position = new Point2D(2, 0), Z = 1 },
                new PlacedObject { Uid = "door_1", ObjectId = "door", Position = new Point2D(3, 0), Z = 1, Events = [EventOf("door_1_0", EventActivation.Touch)] },
                new PlacedObject { Uid = "door_2", ObjectId = "door", Position = new Point2D(0, 1), Z = 1, Events = [EventOf("door_2_0", EventActivation.Interact)] },
                new PlacedObject { Uid = "door_3", ObjectId = "door", Position = new Point2D(1, 2), Z = 1, Events = [EventOf("door_3_0", EventActivation.Touch) with { OnlyWhenObjectId = "crate" }] },
            ],
            CellEvents = [new CellEvent(new GridCell(0, 2), EventOf("trap", EventActivation.Touch))],
        };
        var closet = new MapLayout { Id = "closet", Width = 2, Height = 2, Layers = [new TileLayer { Z = 0, Rows = [[walkable, walkable], [walkable, walkable]] }], Objects = [] };
        var catalog = _testWorld with
        {
            Tiles = new Dictionary<int, TileDefinition>
            {
                [walkable] = new() { Id = walkable, IsWalkable = true, SheetId = "city", Column = 0, Row = 0 },
                [blocked] = new() { Id = blocked, IsWalkable = false, SheetId = "city", Column = 1, Row = 0 },
            },
            Objects = new Dictionary<string, ObjectDefinition>(_testWorld.Objects) { ["crate"] = crate, ["door"] = door },
        };
        return new WarpMaps([new MapEntry("closet", "Closet (closet)"), new MapEntry("hall", "Hall (hall)")], new Dictionary<string, MapLayout> { ["hall"] = hall, ["closet"] = closet }, catalog);
    }

    private static ObjectDefinition OneCellObject(string id, IReadOnlyList<Point2D> outline) => new()
    {
        Id = id,
        Width = 1,
        Height = 1,
        CollisionPolygon = outline,
        Category = "General",
        Tiles = [],
        AbovePlayerZones = [],
    };

    private static MapEvent EventOf(string id, EventActivation activation) => new() { Id = id, Activation = activation, Steps = [] };
}
