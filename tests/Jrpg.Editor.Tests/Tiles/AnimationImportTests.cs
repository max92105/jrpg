using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

// Tiles > Import animations (decision 469) on the test world's forest sheet, FG_Forest_Fall.png: 32 columns of 16 px
// cells. Each test writes its own .tsx text (decision 438: never the pack's files).
public sealed class AnimationImportTests : IDisposable
{
    private const int ForestColumns = 32;

    private readonly CatalogDraft _catalog = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"jrpg-import-{Guid.NewGuid():N}");

    public AnimationImportTests() => Directory.CreateDirectory(_folder);

    public void Dispose() => Directory.Delete(_folder, recursive: true);

    [Fact]
    public void Import_MatchingSheet_AnimatesEachCellAsOneStep()
    {
        var tiles = _catalog.Current.Tiles.Values.Where(tile => tile.SheetId == "fg_forest_fall" && tile.Column + 4 <= ForestColumns).Take(2).ToList();
        var import = new AnimationImport(_catalog);

        import.Import(Tsx("fg_forest_fall.PNG", 16, [.. tiles.Select(tile => Animated(TileIdOf(tile.Column, tile.Row), 4, 140))]));

        Assert.All(tiles, tile => Assert.Equal(new TileAnimation { FrameCount = 4, FrameMilliseconds = 140 }, _catalog.Current.Tiles[tile.Id].Animation));
        Assert.Equal("Animated 2 tiles of fg_forest_fall.", import.Status);
        _catalog.Undo();
        Assert.All(tiles, tile => Assert.Null(_catalog.Current.Tiles[tile.Id].Animation));
    }

    [Fact]
    public void Import_CellsNotTilesYet_MakesThemTiles()
    {
        var cell = (from row in Enumerable.Range(0, 24)
                    from column in Enumerable.Range(0, ForestColumns - 3)
                    select new SheetCell("fg_forest_fall", column, row)).First(cell => _catalog.TileAt(cell) is null);

        new AnimationImport(_catalog).Import(Tsx("FG_Forest_Fall.png", 16, Animated(TileIdOf(cell.Column, cell.Row), 4, 140)));

        Assert.Equal(4, _catalog.TileAt(cell)?.Animation?.FrameCount);
    }

    [Fact]
    public void Import_SameFileTwice_RecordsOneStep()
    {
        var import = new AnimationImport(_catalog);
        var tsx = Tsx("FG_Forest_Fall.png", 16, Animated(0, 4, 140));
        import.Import(tsx);
        var steps = 0;
        _catalog.StepRecorded += (_, _) => steps++;

        import.Import(tsx);

        Assert.Equal(0, steps);
    }

    // The forest image set up a second time at 32 px, as sheet forest_big, which sorts after fg_forest_fall.
    [Fact]
    public void Import_ImageSetUpTwice_AnimatesTheSheetWithTheTsxCut()
    {
        _catalog.SetTilesheet(_catalog.Current.Tilesheets["fg_forest_fall"] with { Id = "forest_big", TileWidth = 32, TileHeight = 32 });
        var import = new AnimationImport(_catalog);

        import.Import(Tsx("FG_Forest_Fall.png", 32, Animated(0, 2, 140)));

        Assert.Equal("Animated 1 tile of forest_big.", import.Status);
        Assert.Equal(2, _catalog.TileAt(new SheetCell("forest_big", 0, 0))?.Animation?.FrameCount);
    }

    [Fact]
    public void Import_NoSheetWithThatImage_ChangesNothingAndSaysSo()
    {
        var before = _catalog.Current;
        var import = new AnimationImport(_catalog);

        import.Import(Tsx("Waterfall B.png", 16, Animated(0, 4, 140)));

        Assert.Same(before, _catalog.Current);
        Assert.Equal("water.tsx: no sheet has the image Waterfall B.png; set it up first.", import.Status);
    }

    [Fact]
    public void Import_OtherTileSize_ChangesNothingAndSaysSo()
    {
        var before = _catalog.Current;
        var import = new AnimationImport(_catalog);

        import.Import(Tsx("FG_Forest_Fall.png", 32, Animated(0, 4, 140)));

        Assert.Same(before, _catalog.Current);
        Assert.Equal("water.tsx: its cut (32x32 px, margin 0, spacing 0) isn't sheet fg_forest_fall's (16x16 px, margin 0, spacing 0).", import.Status);
    }

    [Fact]
    public void Import_AnimationOfAnotherShape_ChangesNothingAndNamesTheTile()
    {
        var before = _catalog.Current;
        var import = new AnimationImport(_catalog);

        import.Import(Tsx("FG_Forest_Fall.png", 16, Animated(0, 4, 140), Animated(31, 2, 140)));

        Assert.Same(before, _catalog.Current);
        Assert.Equal("water.tsx: tile 31: frames must be the tile then the next ones on its row.", import.Status);
    }

    [Fact]
    public void Import_NoAnimatedTile_ChangesNothingAndSaysSo()
    {
        var before = _catalog.Current;
        var import = new AnimationImport(_catalog);

        import.Import(Tsx("FG_Forest_Fall.png", 16));

        Assert.Same(before, _catalog.Current);
        Assert.Equal("water.tsx: no tile is animated.", import.Status);
    }

    private static int TileIdOf(int column, int row) => (row * ForestColumns) + column;

    private static string Animated(int id, int frameCount, int milliseconds) =>
        $"""<tile id="{id}"><animation>{string.Concat(Enumerable.Range(id, frameCount).Select(tileId => $"""<frame tileid="{tileId}" duration="{milliseconds}"/>"""))}</animation></tile>""";

    private string Tsx(string imageFile, int tileSize, params string[] tiles)
    {
        var path = Path.Combine(_folder, "water.tsx");
        File.WriteAllText(path, $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <tileset version="1.10" name="water" tilewidth="{tileSize}" tileheight="{tileSize}" columns="{ForestColumns}">
             <image source="../{imageFile}" width="512" height="384"/>
             {string.Concat(tiles)}
            </tileset>
            """);
        return path;
    }
}
