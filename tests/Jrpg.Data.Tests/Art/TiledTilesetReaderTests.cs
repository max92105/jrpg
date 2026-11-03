using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

// Each test writes its own .tsx text, in Tiled's documented format, to a temporary folder (decision 438: never the
// pack's files).
public sealed class TiledTilesetReaderTests : IDisposable
{
    private const string TwelveColumns = """<tileset version="1.10" name="water" tilewidth="16" tileheight="16" tilecount="96" columns="12" margin="1" spacing="2">""";

    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"jrpg-tsx-{Guid.NewGuid():N}");

    public TiledTilesetReaderTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Fact]
    public void Read_TwoAnimatedTiles_GivesEachCellItsCountAndTime()
    {
        var tileset = Read(TwelveColumns, Animated(0, 4, 140), Animated(13, 8, 70));

        Assert.Equal(
            new Dictionary<(int Column, int Row), TileAnimation>
            {
                [(0, 0)] = new() { FrameCount = 4, FrameMilliseconds = 140 },
                [(1, 1)] = new() { FrameCount = 8, FrameMilliseconds = 70 },
            },
            tileset.Animations);
        Assert.Null(tileset.Refusal);
        Assert.Equal((16, 16, 1, 2), (tileset.TileWidth, tileset.TileHeight, tileset.Margin, tileset.Spacing));
    }

    [Fact]
    public void Read_ImageInASubfolder_KeepsTheFileNameOnly() =>
        Assert.Equal("Waterfall B.png", Read(TwelveColumns).ImageFile);

    [Fact]
    public void Read_NoMarginOrSpacing_ReadsThemAsZero()
    {
        var tileset = Read("""<tileset name="water" tilewidth="16" tileheight="16" columns="12">""");

        Assert.Equal((0, 0), (tileset.Margin, tileset.Spacing));
    }

    [Fact]
    public void Read_TileWithoutAnimation_IsLeftOut()
    {
        var tileset = Read(TwelveColumns, """<tile id="5"><properties><property name="kind" value="rock"/></properties></tile>""");

        Assert.Empty(tileset.Animations);
        Assert.Null(tileset.Refusal);
    }

    [Fact]
    public void Read_FramesNotFromTheTilesOwnId_SetsTheRefusal() =>
        Assert.Equal("tile 3: frames must be the tile then the next ones on its row.", Read(TwelveColumns, Frames(3, (4, 100), (5, 100))).Refusal);

    [Fact]
    public void Read_FramesSkippingAnId_SetsTheRefusal() =>
        Assert.Equal("tile 3: frames must be the tile then the next ones on its row.", Read(TwelveColumns, Frames(3, (3, 100), (5, 100))).Refusal);

    // Tile 10 is on column 10 of 12: its third and fourth frames, ids 12 and 13, are on the next row.
    [Fact]
    public void Read_FramesPastTheRowsEnd_SetsTheRefusal() =>
        Assert.Equal("tile 10: frames must be the tile then the next ones on its row.", Read(TwelveColumns, Animated(10, 4, 100)).Refusal);

    [Fact]
    public void Read_FramesEndingOnTheRowsLastColumn_AreRead() =>
        Assert.Equal(4, Read(TwelveColumns, Animated(8, 4, 100)).Animations[(8, 0)].FrameCount);

    [Fact]
    public void Read_FramesWithTwoDurations_SetsTheRefusal() =>
        Assert.Equal("tile 37: frames last 70 and 140 ms; one time for all is needed.", Read(TwelveColumns, Frames(37, (37, 140), (38, 70))).Refusal);

    [Fact]
    public void Read_OneFrameAnimation_SetsTheRefusal() =>
        Assert.Equal("tile 2: an animation needs at least 2 frames (has 1).", Read(TwelveColumns, Frames(2, (2, 100))).Refusal);

    [Fact]
    public void Read_FrameOfZeroMilliseconds_ThrowsNamingTheFile()
    {
        var exception = Assert.Throws<ContentValidationException>(() => Read(TwelveColumns, Frames(3, (3, 0), (4, 0))));

        Assert.Equal("water.tsx: <frame> needs a duration of at least 1.", exception.Message);
    }

    [Fact]
    public void Read_FrameWithoutDuration_ThrowsNamingTheFile()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => Read(TwelveColumns, """<tile id="3"><animation><frame tileid="3"/><frame tileid="4"/></animation></tile>"""));

        Assert.Equal("water.tsx: <frame> needs a whole number duration.", exception.Message);
    }

    [Fact]
    public void Read_TileAnimatedTwice_SetsTheRefusal() =>
        Assert.Equal("tile 3 is animated twice.", Read(TwelveColumns, Animated(3, 2, 100), Animated(3, 4, 100)).Refusal);

    [Fact]
    public void Read_NoImage_ThrowsNamingTheFile()
    {
        var path = Path.Combine(_folder, "water.tsx");
        File.WriteAllText(path, """<tileset name="water" tilewidth="16" tileheight="16" columns="12"></tileset>""");

        var exception = Assert.Throws<ContentValidationException>(() => TiledTilesetReader.Read(path));

        Assert.Equal("water.tsx: the tileset has no <image source>.", exception.Message);
    }

    [Fact]
    public void Read_RootNotTileset_ThrowsNamingTheFile()
    {
        var path = Path.Combine(_folder, "water.tsx");
        File.WriteAllText(path, """<map width="10" height="10"></map>""");

        var exception = Assert.Throws<ContentValidationException>(() => TiledTilesetReader.Read(path));

        Assert.Equal("water.tsx: not a Tiled tileset (no <tileset> root).", exception.Message);
    }

    [Fact]
    public void Read_MissingColumns_ThrowsNamingTheFile()
    {
        var exception = Assert.Throws<ContentValidationException>(() => Read("""<tileset name="water" tilewidth="16" tileheight="16">"""));

        Assert.Equal("water.tsx: <tileset> needs a whole number columns.", exception.Message);
    }

    [Fact]
    public void Read_NotXml_ThrowsNamingTheFile()
    {
        var path = Path.Combine(_folder, "water.tsx");
        File.WriteAllText(path, "PNG, not XML");

        var exception = Assert.Throws<ContentValidationException>(() => TiledTilesetReader.Read(path));

        Assert.StartsWith("water.tsx: not a Tiled tileset (", exception.Message, StringComparison.Ordinal);
    }

    private static string Animated(int id, int frameCount, int milliseconds) =>
        Frames(id, [.. Enumerable.Range(id, frameCount).Select(tileId => (tileId, milliseconds))]);

    private static string Frames(int id, params (int TileId, int Duration)[] frames) =>
        $"""<tile id="{id}"><animation>{string.Concat(frames.Select(frame => $"""<frame tileid="{frame.TileId}" duration="{frame.Duration}"/>"""))}</animation></tile>""";

    private TiledTileset Read(string tilesetTag, params string[] tiles)
    {
        var path = Path.Combine(_folder, "water.tsx");
        File.WriteAllText(path, $"""
            <?xml version="1.0" encoding="UTF-8"?>
            {tilesetTag}
             <image source="../Waterfall/Waterfall B.png" width="192" height="128"/>
             {string.Concat(tiles)}
            </tileset>
            """);
        return TiledTilesetReader.Read(path);
    }
}
