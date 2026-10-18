using Jrpg.ContentConverter.Tiles;

namespace Jrpg.ContentConverter.Tests.Tiles;

public sealed class TilesheetConversionTests
{
    private static readonly LegacyTilesheetJson _forest = new()
    {
        File = "FG_Forest_Fall.png",
        Name = "FG_Forest_Fall",
        TileWidth = 16,
        TileHeight = 16,
        Margin = 1,
        Spacing = 2,
    };

    private readonly ConversionReport _report = new();

    [Fact]
    public void Convert_ConfiguredSheet_KeepsItsGrid()
    {
        var sheet = Assert.Single(TilesheetConversion.Convert([_forest], ["FG_Forest_Fall.png"], _report));

        Assert.Equal(("fg_forest_fall", "FG_Forest_Fall.png", 16, 16, 1, 2), (sheet.Id, sheet.File, sheet.TileWidth, sheet.TileHeight, sheet.Margin, sheet.Spacing));
        Assert.Empty(_report.Lines);
    }

    [Fact]
    public void Convert_UnconfiguredSheet_Uses48PixelGridAndReports()
    {
        var sheet = Assert.Single(TilesheetConversion.Convert([], ["RA_Jungle.png", "RA_Jungle.png"], _report));

        Assert.Equal(("ra_jungle", "RA_Jungle.png", 48, 48, 0, 0), (sheet.Id, sheet.File, sheet.TileWidth, sheet.TileHeight, sheet.Margin, sheet.Spacing));
        Assert.Equal(["Tiles/Tilesheets.json: changed tilesheet RA_Jungle.png to the legacy 48 px grid, since it has no settings."], _report.Lines);
    }

    [Fact]
    public void Convert_ConfiguredSheetNoTileUses_SkipsAndReports()
    {
        var sheets = TilesheetConversion.Convert([_forest, _forest with { File = "Floor.png" }], ["FG_Forest_Fall.png"], _report);

        Assert.Equal(["fg_forest_fall"], sheets.Select(sheet => sheet.Id));
        Assert.Equal(["Tiles/Tilesheets.json: skipped tilesheet Floor.png, which no tile uses."], _report.Lines);
    }

    [Fact]
    public void Convert_FileCaseDiffersFromSettings_UsesTheSettings()
    {
        var sheet = Assert.Single(TilesheetConversion.Convert([_forest], ["fg_forest_fall.png"], _report));

        Assert.Equal(16, sheet.TileWidth);
    }

    [Fact]
    public void Convert_FileWithFolder_MatchesTheSettingsByFileName()
    {
        var sheet = Assert.Single(TilesheetConversion.Convert([_forest], ["Tiles/FG_Forest_Fall.png"], _report));

        Assert.Equal(("FG_Forest_Fall.png", 16), (sheet.File, sheet.TileWidth));
    }

    [Fact]
    public void Convert_TwoFilesSameId_ThrowsNamingBoth()
    {
        var exception = Assert.Throws<ConversionException>(() => TilesheetConversion.Convert([], ["Water1.png", "Water_1.png"], _report));

        Assert.Equal("Tiles/Tilesheets.json: sheets Water1.png and Water_1.png both become tilesheet id 'water_1'.", exception.Message);
    }

    [Fact]
    public void Convert_RepositoryTileset_ListsEveryReferencedSheet()
    {
        var legacyTiles = LegacyFileReader.Read<Dictionary<string, LegacyTileJson>>(TestPaths.LegacyAssets, TileConversion.SourceFile);
        var legacySheets = LegacyFileReader.Read<IReadOnlyList<LegacyTilesheetJson>>(TestPaths.LegacyAssets, TilesheetConversion.SourceFile);

        var sheets = TilesheetConversion.Convert(legacySheets, legacyTiles.Values.Select(tile => tile.Tilesheet!), _report);
        var tiles = TileConversion.Convert(legacyTiles);

        Assert.Equal(10, sheets.Count);
        Assert.Equal(sheets.Select(sheet => sheet.Id).Order(StringComparer.Ordinal), tiles.Select(tile => tile.SheetId).Distinct().Order(StringComparer.Ordinal));
    }
}
