using Jrpg.ContentConverter.Objects;
using Jrpg.Core.Geometry;

namespace Jrpg.ContentConverter.Tests.Objects;

public sealed class ObjectConversionTests
{
    private static readonly LegacyObjectJson _chest = new()
    {
        Id = 9,
        Name = "Chest",
        Category = "Chests",
        Width = 1,
        Height = 1,
        Tiles = [new LegacyTileGroupEntryJson(X: 0, Y: 0, TileId: 40, Z: 0)],
        CollisionPolygon = [new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1)],
        AbovePlayerZones = [],
        IsNpc = false,
        Meta = new Dictionary<string, string>(),
    };

    [Fact]
    public void Convert_StaleSize_UsesTileGroupBoundsAndReports()
    {
        // The legacy tree: its tile group spans columns 0..3 and rows 1..4, while the file says 1x1.
        var tree = _chest with
        {
            Name = "Tree",
            Tiles = [new(X: 0, Y: 1, TileId: 2, Z: 0), new(X: 3, Y: 1, TileId: 5, Z: 0), new(X: 1, Y: 4, TileId: 10, Z: 0)],
        };
        var report = new ConversionReport();

        var converted = Assert.Single(ObjectConversion.Convert([tree], report).Definitions);

        Assert.Equal(("tree", 4, 4), (converted.Id, converted.Width, converted.Height));
        Assert.Equal(["Objects/Objects.json: changed object 'Tree': stale size 1x1 replaced by its tile group's 4x4."], report.Lines);
    }

    [Fact]
    public void Convert_SizeMatchesTileGroup_ReportsNothingAndKeepsThePolygon()
    {
        var report = new ConversionReport();

        var converted = Assert.Single(ObjectConversion.Convert([_chest], report).Definitions);

        Assert.Equal(("chest", 1, 1), (converted.Id, converted.Width, converted.Height));
        Assert.Equal(_chest.CollisionPolygon, converted.CollisionPolygon);
        Assert.Empty(report.Lines);
    }

    [Fact]
    public void Convert_Objects_MapsEachLegacyIdToTheNewId()
    {
        var converted = ObjectConversion.Convert([_chest, _chest with { Id = 3, Name = "Door_Open" }], new ConversionReport());

        Assert.Equal(new Dictionary<int, string> { [9] = "chest", [3] = "door_open" }, converted.IdsByLegacyId);
    }

    [Fact]
    public void Convert_TwoObjectsSameLegacyId_ThrowsNamingTheId()
    {
        var exception = Assert.Throws<ConversionException>(
            () => ObjectConversion.Convert([_chest, _chest with { Name = "Chest Opened" }], new ConversionReport()));

        Assert.Equal("Objects/Objects.json: legacy id 9 is used by more than one object.", exception.Message);
    }

    [Fact]
    public void Convert_TwoNamesSameId_ThrowsNamingBoth()
    {
        var exception = Assert.Throws<ConversionException>(
            () => ObjectConversion.Convert([_chest with { Name = "House1" }, _chest with { Name = "House_1" }], new ConversionReport()));

        Assert.Equal("Objects/Objects.json: objects 'House1' and 'House_1' both become id 'house_1'.", exception.Message);
    }

    [Fact]
    public void Convert_NoTiles_ThrowsNamingTheObject()
    {
        var exception = Assert.Throws<ConversionException>(
            () => ObjectConversion.Convert([_chest with { Tiles = [] }], new ConversionReport()));

        Assert.Equal("Objects/Objects.json: object 'Chest' has no tiles, so it has no size.", exception.Message);
    }

    [Fact]
    public void Convert_Npc_ThrowsNamingTheObject()
    {
        var exception = Assert.Throws<ConversionException>(
            () => ObjectConversion.Convert([_chest with { IsNpc = true }], new ConversionReport()));

        Assert.Equal("Objects/Objects.json: object 'Chest' is an NPC, which the converter doesn't support yet.", exception.Message);
    }

    [Fact]
    public void Convert_Meta_ThrowsNamingTheObject()
    {
        var chest = _chest with { Meta = new Dictionary<string, string> { ["sound"] = "creak.wav" } };

        var exception = Assert.Throws<ConversionException>(() => ObjectConversion.Convert([chest], new ConversionReport()));

        Assert.Equal("Objects/Objects.json: object 'Chest' has meta data, which the converter doesn't support yet.", exception.Message);
    }

    [Fact]
    public void Convert_NullTile_ThrowsNamingTheObject()
    {
        var chest = _chest with { Tiles = [_chest.Tiles[0], null!] };

        var exception = Assert.Throws<ConversionException>(() => ObjectConversion.Convert([chest], new ConversionReport()));

        Assert.Equal("Objects/Objects.json: object 'Chest' has a null tile.", exception.Message);
    }

    [Fact]
    public void Convert_RepositoryObjects_ConvertsTwelveWithSnakeCaseIds()
    {
        var legacyObjects = LegacyFileReader.Read<IReadOnlyList<LegacyObjectJson>>(TestPaths.LegacyAssets, ObjectConversion.SourceFile);

        var objects = ObjectConversion.Convert(legacyObjects, new ConversionReport()).Definitions;

        Assert.Equal(
            ["chest", "chest_opened", "door_close", "door_open", "house_1", "house_2", "house_3", "house_4", "house_5", "tree", "tree_2", "tree_3"],
            objects.Select(definition => definition.Id));
    }
}
