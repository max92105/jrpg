using Jrpg.ContentConverter.Maps;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;

namespace Jrpg.ContentConverter.Tests.Maps;

public sealed class MapConversionTests
{
    private const string SourceFile = "Maps/Interior1.json";

    private static readonly Dictionary<int, string> _objectIds = new() { [9] = "chest" };

    private readonly MapConversionContext _context = new(SourceFile, _objectIds, new ConversionReport());

    private static readonly LegacyPlacedObjectJson _chest = new()
    {
        Id = 9,
        X = 2.5,
        Y = 1,
        Z = -3,
        Meta = null,
        ZOverrides = new Dictionary<string, int>(),
        Uid = "9@2.5,1",
    };

    [Fact]
    public void Convert_LegacyMap_TakesIdFromFileNameAndKeepsSize()
    {
        var layout = MapConversion.Convert(LegacyMap(width: 3, height: 2), _context);

        Assert.Equal(("interior_1", 3, 2), (layout.Id, layout.Width, layout.Height));
    }

    [Fact]
    public void Convert_TileIdZero_IsDropped()
    {
        var map = LegacyMap(width: 2, height: 1, Tile(id: 5, x: 0, y: 0, z: -5), Tile(id: 0, x: 1, y: 0, z: -5), Tile(id: 0, x: 1, y: 0, z: 0));

        var layout = MapConversion.Convert(map, _context);

        var layer = Assert.Single(layout.Layers);
        Assert.Equal([5, TileLayer.EmptyTileId], layer.Rows[0]);
    }

    [Fact]
    public void Convert_LayersOrderedByZ()
    {
        var map = LegacyMap(width: 1, height: 1, Tile(id: 7, x: 0, y: 0, z: -4), Tile(id: 3, x: 0, y: 0, z: -5), Tile(id: 8, x: 0, y: 0, z: 2));

        var layout = MapConversion.Convert(map, _context);

        Assert.Equal([3, 7, 8], layout.Layers.Select(layer => layer.TileIdAt(new GridCell(0, 0))));
    }

    [Fact]
    public void Convert_TwoTilesOnSameCellAndLayer_ThrowsNamingTheCell()
    {
        var map = LegacyMap(width: 2, height: 2, Tile(id: 3, x: 1, y: 1, z: -5), Tile(id: 4, x: 1, y: 1, z: -5));

        var exception = Assert.Throws<ConversionException>(() => MapConversion.Convert(map, _context));

        Assert.Equal("Maps/Interior1.json: cell (1, 1) holds two tiles on layer z -5.", exception.Message);
    }

    [Fact]
    public void Convert_TileOutsideMap_ThrowsNamingTheTile()
    {
        var map = LegacyMap(width: 2, height: 2, Tile(id: 3, x: 2, y: 0, z: -5));

        var exception = Assert.Throws<ConversionException>(() => MapConversion.Convert(map, _context));

        Assert.Equal("Maps/Interior1.json: tile 3 at (2, 0) is outside the map.", exception.Message);
    }

    [Fact]
    public void Convert_PlacedObject_KeepsUidAndPositionWithNewObjectId()
    {
        var map = LegacyMap(width: 4, height: 4) with { Objects = [_chest] };

        var placed = Assert.Single(MapConversion.Convert(map, _context).Objects);

        // Records holding lists compare those lists by reference, so the fields are compared one by one.
        Assert.Equal(("9@2.5,1", "chest", new Point2D(2.5, 1)), (placed.Uid, placed.ObjectId, placed.Position));
        Assert.Empty(placed.Events);
    }

    [Fact]
    public void Convert_LiveObjectWithoutUid_ThrowsNamingTheFile()
    {
        var map = LegacyMap(width: 4, height: 4) with { Objects = [_chest with { Uid = null }] };

        var exception = Assert.Throws<ConversionException>(() => MapConversion.Convert(map, _context));

        Assert.Equal("Maps/Interior1.json: object 9 at (2.5, 1) has no uid.", exception.Message);
    }

    [Fact]
    public void Convert_ObjectIdMissingFromObjectList_ThrowsNamingTheObject()
    {
        var map = LegacyMap(width: 4, height: 4) with { Objects = [_chest with { Id = 42 }] };

        var exception = Assert.Throws<ConversionException>(() => MapConversion.Convert(map, _context));

        Assert.Equal("Maps/Interior1.json: object 42 at (2.5, 1) uses an object id that isn't in the object list.", exception.Message);
    }

    [Fact]
    public void Convert_NullTile_ThrowsNamingTheFile()
    {
        var map = LegacyMap(width: 1, height: 1) with { Tiles = [null!] };

        var exception = Assert.Throws<ConversionException>(() => MapConversion.Convert(map, _context));

        Assert.Equal("Maps/Interior1.json: the tile list holds a null entry.", exception.Message);
    }

    [Fact]
    public void Convert_NullObject_ThrowsNamingTheFile()
    {
        var map = LegacyMap(width: 1, height: 1) with { Objects = [_chest, null!] };

        var exception = Assert.Throws<ConversionException>(() => MapConversion.Convert(map, _context));

        Assert.Equal("Maps/Interior1.json: the object list holds a null entry.", exception.Message);
    }

    [Fact]
    public void Convert_PlacedObjectWithMeta_ThrowsNamingTheObject()
    {
        var chest = _chest with { Meta = new Dictionary<string, string> { ["locked"] = "true" } };
        var map = LegacyMap(width: 4, height: 4) with { Objects = [chest] };

        var exception = Assert.Throws<ConversionException>(() => MapConversion.Convert(map, _context));

        Assert.Equal("Maps/Interior1.json: object 9 at (2.5, 1) has meta data, which the converter doesn't support yet.", exception.Message);
    }

    [Fact]
    public void Convert_LegacySingleEvent_ThrowsNamingTheFile()
    {
        var chest = _chest with { Event = System.Text.Json.JsonDocument.Parse("""{ "activation": "Interact" }""").RootElement };
        var map = LegacyMap(width: 4, height: 4) with { Objects = [chest] };

        var exception = Assert.Throws<ConversionException>(() => MapConversion.Convert(map, _context));

        Assert.StartsWith("Maps/Interior1.json: object 9 at (2.5, 1) has the old single \"event\" field", exception.Message, StringComparison.Ordinal);
    }

    private static LegacyMapTileJson Tile(int id, int x, int y, int z) => new() { Id = id, X = x, Y = y, Z = z };

    private static LegacyMapJson LegacyMap(int width, int height, params LegacyMapTileJson[] tiles) => new()
    {
        Name = "Interior1",
        Width = width,
        Height = height,
        RandomEncounters = false,
        Tiles = tiles,
        UsedTileIds = [],
        Objects = [],
    };
}
