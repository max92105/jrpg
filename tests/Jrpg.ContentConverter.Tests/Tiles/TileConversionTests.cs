using Jrpg.ContentConverter.Tiles;

namespace Jrpg.ContentConverter.Tests.Tiles;

public sealed class TileConversionTests
{
    private static readonly LegacyTileJson _grass = new()
    {
        Id = 1,
        Name = "Grass",
        IsWalkable = true,
        FrameDuration = 0.0,
        Tilesheet = "RA_Ground_Tiles.png",
        SheetX = 2,
        SheetY = 5,
    };

    [Fact]
    public void Convert_KeyDisagreesWithInnerId_ThrowsNamingTheKey()
    {
        var exception = Assert.Throws<ConversionException>(
            () => TileConversion.Convert(new Dictionary<string, LegacyTileJson> { ["7"] = _grass }));

        Assert.Equal("Tiles/Tileset.json: key '7' holds tile id 1; they must be equal.", exception.Message);
    }

    [Fact]
    public void Convert_KeyIsNotANumber_ThrowsNamingTheKey()
    {
        var exception = Assert.Throws<ConversionException>(
            () => TileConversion.Convert(new Dictionary<string, LegacyTileJson> { ["grass"] = _grass }));

        Assert.Equal("Tiles/Tileset.json: key 'grass' isn't a tile id.", exception.Message);
    }

    [Fact]
    public void Convert_NullEntry_ThrowsNamingTheKey()
    {
        var exception = Assert.Throws<ConversionException>(
            () => TileConversion.Convert(new Dictionary<string, LegacyTileJson> { ["1"] = _grass, ["2"] = null! }));

        Assert.Equal("Tiles/Tileset.json: key '2' holds null.", exception.Message);
    }

    [Theory]
    [InlineData(nameof(LegacyTileJson.CollisionPolygon))]
    [InlineData(nameof(LegacyTileJson.Sprite))]
    [InlineData(nameof(LegacyTileJson.Frames))]
    public void Convert_TileWithUnsupportedField_ThrowsNamingTheTile(string field)
    {
        var tile = field switch
        {
            nameof(LegacyTileJson.CollisionPolygon) => _grass with { CollisionPolygon = ["0,0", "1,0", "1,1"] },
            nameof(LegacyTileJson.Sprite) => _grass with { Sprite = "Grass1.png" },
            _ => _grass with { Frames = ["Water1.png"] },
        };

        var exception = Assert.Throws<ConversionException>(
            () => TileConversion.Convert(new Dictionary<string, LegacyTileJson> { ["1"] = tile }));

        Assert.Equal(
            "Tiles/Tileset.json: tile 1 has a sprite, frames or a collision polygon, which the converter doesn't support yet.",
            exception.Message);
    }

    [Fact]
    public void Convert_KeysOutOfOrder_ReturnsTilesOrderedById()
    {
        var legacyTiles = new Dictionary<string, LegacyTileJson>
        {
            ["10"] = _grass with { Id = 10, IsWalkable = false },
            ["2"] = _grass with { Id = 2 },
        };

        var tiles = TileConversion.Convert(legacyTiles);

        Assert.Equal([2, 10], tiles.Select(tile => tile.Id));
        Assert.Equal([true, false], tiles.Select(tile => tile.IsWalkable));
    }

    [Fact]
    public void Convert_RepositoryTileset_Converts283WalkableTiles()
    {
        var legacyTiles = LegacyFileReader.Read<Dictionary<string, LegacyTileJson>>(TestPaths.LegacyAssets, TileConversion.SourceFile);

        var tiles = TileConversion.Convert(legacyTiles);

        Assert.Equal(283, tiles.Count);
        Assert.All(tiles, tile => Assert.True(tile.IsWalkable));
    }
}
