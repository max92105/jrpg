using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class TileCatalogValidatorTests
{
    private const string FileName = "generated/tiles.json";

    private static readonly TileDefinition _grass = new() { Id = 1, IsWalkable = true, SheetId = "city", Column = 2, Row = 5 };
    private static readonly HashSet<string> _sheetIds = ["city"];

    [Fact]
    public void Validate_ValidTiles_DoesNotThrow()
    {
        var exception = Record.Exception(() => TileCatalogValidator.Validate(FileName, [_grass, _grass with { Id = 2 }], _sheetIds));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_TileIdZero_ThrowsNamingTheField()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => TileCatalogValidator.Validate(FileName, [_grass, _grass with { Id = 0 }], _sheetIds));

        Assert.Equal("generated/tiles.json: tile '0': id must be at least 1, since 0 means an empty cell (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => TileCatalogValidator.Validate(FileName, [_grass, _grass with { IsWalkable = false }], _sheetIds));

        Assert.Equal("generated/tiles.json: tile id '1' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_NullEntry_ThrowsNamingTheIndex()
    {
        var exception = Assert.Throws<ContentValidationException>(() => TileCatalogValidator.Validate(FileName, [_grass, null!], _sheetIds));

        Assert.Equal("generated/tiles.json: entry 1 is null.", exception.Message);
    }

    [Fact]
    public void Validate_UnknownSheet_ThrowsNamingTheTile()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => TileCatalogValidator.Validate(FileName, [_grass with { SheetId = "jungle" }], _sheetIds));

        Assert.Equal("generated/tiles.json: tile '1': sheetId 'jungle' isn't a tilesheet.", exception.Message);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public void Validate_NegativeCell_ThrowsNamingTheTile(int column, int row)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => TileCatalogValidator.Validate(FileName, [_grass with { Column = column, Row = row }], _sheetIds));

        Assert.Equal($"generated/tiles.json: tile '1': column and row must not be negative (was {column}, {row}).", exception.Message);
    }
}
