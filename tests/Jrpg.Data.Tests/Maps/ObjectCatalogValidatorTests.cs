using System.Globalization;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class ObjectCatalogValidatorTests
{
    private const string FileName = "objects.json";

    private static readonly HashSet<int> _tileIds = [1, 2];

    private static readonly ObjectDefinition _chest = new()
    {
        Id = "chest",
        Width = 1,
        Height = 2,
        CollisionPolygon = [new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1)],
        Category = "Chests",
        Tiles = [new ObjectTile(0, 0, 1), new ObjectTile(0, 1, 2)],
        AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 0.5)]],
    };

    // An NPC (ADR 0015): drawn from a field sprite, so no tiles and no zones.
    private static readonly ObjectDefinition _villager = _chest with
    {
        Id = "villager",
        Height = 1,
        Category = "NPC",
        Tiles = [],
        FieldSpriteFile = "npc_placeholder.png",
        AbovePlayerZones = [],
    };

    [Fact]
    public void Validate_ValidObjects_DoesNotThrow()
    {
        var exception = Record.Exception(
            () => ObjectCatalogValidator.Validate(FileName, [_chest, _chest with { Id = "rug", CollisionPolygon = [], AbovePlayerZones = [] }], _tileIds));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_TwoPointPolygon_ThrowsNamingTheObject()
    {
        var chest = _chest with { CollisionPolygon = [new Point2D(0, 0), new Point2D(1, 1)] };

        var exception = Assert.Throws<ContentValidationException>(() => ObjectCatalogValidator.Validate(FileName, [chest], _tileIds));

        Assert.Equal("objects.json: object 'chest': collisionPolygon must be empty or have at least 3 points (had 2).", exception.Message);
    }

    [Theory]
    [InlineData(1.5, 0.5)]
    [InlineData(0.5, -0.25)]
    public void Validate_PointOutsideUnitSquare_ThrowsNamingTheObject(double x, double y)
    {
        var chest = _chest with { CollisionPolygon = [new Point2D(0, 0), new Point2D(x, y), new Point2D(1, 1)] };

        var exception = Assert.Throws<ContentValidationException>(() => ObjectCatalogValidator.Validate(FileName, [chest], _tileIds));

        Assert.StartsWith(
            string.Create(CultureInfo.InvariantCulture, $"objects.json: object 'chest': collisionPolygon point ({x}, {y}) must be within 0..1"),
            exception.Message,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void Validate_EmptySize_ThrowsNamingTheObject(int width, int height)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_chest with { Width = width, Height = height }], _tileIds));

        Assert.Equal($"objects.json: object 'chest': width and height must be at least 1 (was {width}x{height}).", exception.Message);
    }

    [Fact]
    public void Validate_EmptyCategory_ThrowsNamingTheObject()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_chest with { Category = "" }], _tileIds));

        Assert.Equal("objects.json: object 'chest': category must not be empty.", exception.Message);
    }

    [Fact]
    public void Validate_NeitherTilesNorSprite_ThrowsNamingTheObject()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_chest with { Tiles = [] }], _tileIds));

        Assert.Equal("objects.json: object 'chest': tiles or fieldSpriteFile must be set; an object without either draws nothing.", exception.Message);
    }

    [Fact]
    public void Validate_SpriteObjectWithTiles_ThrowsNamingTheObject()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_villager with { Tiles = _chest.Tiles }], _tileIds));

        Assert.Equal("objects.json: object 'villager': tiles must be empty for an object with a fieldSpriteFile; it is drawn from the sprite.", exception.Message);
    }

    [Fact]
    public void Validate_SpriteObjectWithZones_ThrowsNamingTheObject()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_villager with { AbovePlayerZones = _chest.AbovePlayerZones }], _tileIds));

        Assert.Equal("objects.json: object 'villager': abovePlayerZones aren't drawn for a field-sprite object.", exception.Message);
    }

    [Fact]
    public void Validate_SpriteObjectWithoutTiles_Passes()
    {
        var exception = Record.Exception(() => ObjectCatalogValidator.Validate(FileName, [_villager], _tileIds));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_UnknownTileId_ThrowsNamingTheObject()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_chest with { Tiles = [new ObjectTile(0, 0, 9)] }], _tileIds));

        Assert.Equal("objects.json: object 'chest': tile (0, 0) has tile id 9, which isn't a tile.", exception.Message);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 2)]
    [InlineData(-1, 0)]
    public void Validate_TileOutsideBounds_ThrowsNamingTheObject(int x, int y)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_chest with { Tiles = [new ObjectTile(x, y, 1)] }], _tileIds));

        Assert.Equal($"objects.json: object 'chest': tile ({x}, {y}) must be inside the object's 1x2 cells.", exception.Message);
    }

    [Fact]
    public void Validate_TwoTilesOnOneCell_ThrowsNamingTheObject()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => ObjectCatalogValidator.Validate(FileName, [_chest with { Tiles = [new ObjectTile(0, 1, 1), new ObjectTile(0, 1, 2)] }], _tileIds));

        Assert.Equal("objects.json: object 'chest': tile (0, 1) is listed more than once; an object has one tile per cell.", exception.Message);
    }

    [Fact]
    public void Validate_ZoneWithTwoPoints_ThrowsNamingTheZone()
    {
        var chest = _chest with { AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 1)]] };

        var exception = Assert.Throws<ContentValidationException>(() => ObjectCatalogValidator.Validate(FileName, [chest], _tileIds));

        Assert.Equal("objects.json: object 'chest': abovePlayerZones[0] must have at least 3 points.", exception.Message);
    }

    [Fact]
    public void Validate_ZonePointOutsideUnitSquare_ThrowsNamingTheZone()
    {
        var chest = _chest with { AbovePlayerZones = [[new Point2D(0, 0), new Point2D(2, 0), new Point2D(1, 1)]] };

        var exception = Assert.Throws<ContentValidationException>(() => ObjectCatalogValidator.Validate(FileName, [chest], _tileIds));

        Assert.StartsWith("objects.json: object 'chest': abovePlayerZones[0] point (2, 0) must be within 0..1", exception.Message, StringComparison.Ordinal);
    }
}
