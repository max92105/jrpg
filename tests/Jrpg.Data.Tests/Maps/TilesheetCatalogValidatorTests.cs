using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Data.Maps;

namespace Jrpg.Data.Tests.Maps;

public sealed class TilesheetCatalogValidatorTests
{
    private const string FileName = "generated/tilesheets.json";

    private static readonly TilesheetDefinition _city = new()
    {
        Id = "city",
        File = "City.png",
        TileWidth = 48,
        TileHeight = 48,
        Margin = 0,
        Spacing = 0,
    };

    [Fact]
    public void Validate_ValidSheets_DoesNotThrow()
    {
        var exception = Record.Exception(() => TilesheetCatalogValidator.Validate(FileName, [_city, _city with { Id = "forest" }]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_ZeroTileWidth_ThrowsNamingTheSheet()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => TilesheetCatalogValidator.Validate(FileName, [_city with { TileWidth = 0 }]));

        Assert.Equal("generated/tilesheets.json: tilesheet 'city': tileWidth and tileHeight must be at least 1 (was 0 x 48).", exception.Message);
    }

    [Fact]
    public void Validate_EmptyFile_ThrowsNamingTheSheet()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => TilesheetCatalogValidator.Validate(FileName, [_city with { File = " " }]));

        Assert.Equal("generated/tilesheets.json: tilesheet 'city': file must not be empty.", exception.Message);
    }

    [Fact]
    public void Validate_NegativeSpacing_ThrowsNamingTheSheet()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => TilesheetCatalogValidator.Validate(FileName, [_city with { Spacing = -1 }]));

        Assert.Equal("generated/tilesheets.json: tilesheet 'city': margin and spacing must not be negative (was 0 and -1).", exception.Message);
    }

    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => TilesheetCatalogValidator.Validate(FileName, [_city, _city with { File = "City2.png" }]));

        Assert.Equal("generated/tilesheets.json: tilesheet id 'city' is used more than once.", exception.Message);
    }
}
