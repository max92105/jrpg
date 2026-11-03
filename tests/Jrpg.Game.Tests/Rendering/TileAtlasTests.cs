using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Rendering;

public sealed class TileAtlasTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    [Fact]
    public void ImageOf_EveryTestWorldTile_IsACellOfItsSheet()
    {
        var atlas = new TileAtlas(_catalog);

        Assert.All(_catalog.Tiles.Values, tile =>
        {
            var sheet = _catalog.Tilesheets[tile.SheetId];
            var expected = new Rectangle(sheet.CellLeft(tile.Column), sheet.CellTop(tile.Row), sheet.TileWidth, sheet.TileHeight);
            Assert.Equal(new TileImage(sheet.File, expected), atlas.ImageOf(tile.Id));
        });
    }

    // A tile of the test world's forest sheet, given four frames of 140 ms: the next cell shows at 140 ms.
    [Fact]
    public void ImageAt_AnimatedTileAtTheFrameTime_IsTheNextCellToTheRight()
    {
        var (atlas, tile, sheet) = AtlasWithAnimated();

        var image = atlas.ImageAt(tile.Id, TimeSpan.FromMilliseconds(140));

        Assert.Equal(new Rectangle(sheet.CellLeft(tile.Column + 1), sheet.CellTop(tile.Row), sheet.TileWidth, sheet.TileHeight), image.Source);
    }

    [Fact]
    public void ImageAt_StillTile_IsItsCellAtAnyTime()
    {
        var atlas = new TileAtlas(_catalog);
        var tile = _catalog.Tiles.Values.First(tile => tile.Animation is null);

        Assert.Equal(atlas.ImageOf(tile.Id), atlas.ImageAt(tile.Id, TimeSpan.FromMilliseconds(12_345)));
    }

    [Fact]
    public void ImageOf_AnimatedTile_IsTheFirstFrame()
    {
        var (atlas, tile, sheet) = AtlasWithAnimated();

        Assert.Equal(new Rectangle(sheet.CellLeft(tile.Column), sheet.CellTop(tile.Row), sheet.TileWidth, sheet.TileHeight), atlas.ImageOf(tile.Id).Source);
    }

    private static (TileAtlas Atlas, TileDefinition Tile, TilesheetDefinition Sheet) AtlasWithAnimated()
    {
        var tile = _catalog.Tiles.Values.First(tile => tile.SheetId == "fg_forest_fall" && tile.Column == 0) with
        {
            Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 140 },
        };
        var catalog = _catalog with { Tiles = new Dictionary<int, TileDefinition>(_catalog.Tiles) { [tile.Id] = tile } };
        return (new TileAtlas(catalog), tile, catalog.Tilesheets[tile.SheetId]);
    }
}
