using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

public sealed class CollisionShapesTests
{
    private const int Walkable = 1;
    private const int Blocked = 2;

    // A 2 x 3 house with a triangle outline over its lower half and one zone over its top; a sign with no shapes.
    private static readonly ObjectDefinition _house = new()
    {
        Id = "house",
        Width = 2,
        Height = 3,
        Category = "Buildings",
        Tiles = [],
        CollisionPolygon = [new Point2D(0, 0.5), new Point2D(1, 0.5), new Point2D(0.5, 1)],
        AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 0.5)]],
    };

    private static readonly ObjectDefinition _sign = _house with { Id = "sign", Width = 1, Height = 1, CollisionPolygon = [], AbovePlayerZones = [] };

    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")) with
    {
        Tiles = new Dictionary<int, TileDefinition>
        {
            [Walkable] = new() { Id = Walkable, IsWalkable = true, SheetId = "city", Column = 0, Row = 0 },
            [Blocked] = new() { Id = Blocked, IsWalkable = false, SheetId = "city", Column = 1, Row = 0 },
        },
        Objects = new Dictionary<string, ObjectDefinition> { ["house"] = _house, ["sign"] = _sign },
    };

    [Fact]
    public void Of_CellWithBlockedTile_IsBlocked()
    {
        var shapes = CollisionShapes.Of(Layout(cell => cell == new GridCell(2, 3) ? Blocked : Walkable), _catalog);

        Assert.Equal([new GridCell(2, 3)], shapes.BlockedCells);
    }

    [Fact]
    public void Of_CellWithoutTiles_IsBlocked()
    {
        var shapes = CollisionShapes.Of(Layout(cell => cell == new GridCell(0, 0) ? TileLayer.EmptyTileId : Walkable), _catalog);

        Assert.Equal([new GridCell(0, 0)], shapes.BlockedCells);
    }

    // (0.5, 1) of a 2 x 3 object is 1 cell right and 3 down of its corner (4, 5).
    [Fact]
    public void Of_ObjectWithOutline_PlacesItInMapCells()
    {
        var shapes = CollisionShapes.Of(Layout(_ => Walkable, Placed("house", 4, 5)), _catalog);

        Assert.Equal([new Point2D(4, 6.5), new Point2D(6, 6.5), new Point2D(5, 8)], Assert.Single(shapes.Outlines));
    }

    [Fact]
    public void Of_ObjectWithZones_ListsEachZoneInMapCells()
    {
        var shapes = CollisionShapes.Of(Layout(_ => Walkable, Placed("house", 4, 5)), _catalog);

        Assert.Equal([new Point2D(4, 5), new Point2D(6, 5), new Point2D(6, 6.5)], Assert.Single(shapes.Zones));
    }

    [Fact]
    public void Of_ObjectWithoutOutline_AddsNoOutline()
    {
        var shapes = CollisionShapes.Of(Layout(_ => Walkable, Placed("sign", 1, 1)), _catalog);

        Assert.Empty(shapes.Outlines);
        Assert.Empty(shapes.Zones);
    }

    // A 10 x 10 map of one layer, each cell's tile chosen by the caller.
    private static MapLayout Layout(Func<GridCell, int> tileAt, params PlacedObject[] objects) => new()
    {
        Id = "glade",
        Width = 10,
        Height = 10,
        Layers = [new TileLayer { Z = 0, Rows = [.. Enumerable.Range(0, 10).Select(y => (IReadOnlyList<int>)[.. Enumerable.Range(0, 10).Select(x => tileAt(new GridCell(x, y)))])] }],
        Objects = objects,
    };

    private static PlacedObject Placed(string objectId, int x, int y) =>
        new() { Uid = $"{objectId}_1", ObjectId = objectId, Position = new Point2D(x, y), Z = 1 };
}
