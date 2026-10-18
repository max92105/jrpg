using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Rendering;

public sealed class TileAtlasTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void ImageOf_EveryRepositoryTile_IsACellOfItsSheet()
    {
        var atlas = new TileAtlas(_catalog);

        Assert.All(_catalog.Tiles.Values, tile =>
        {
            var sheet = _catalog.Tilesheets[tile.SheetId];
            var expected = new Rectangle(sheet.CellLeft(tile.Column), sheet.CellTop(tile.Row), sheet.TileWidth, sheet.TileHeight);
            Assert.Equal(new TileImage(sheet.File, expected), atlas.ImageOf(tile.Id));
        });
    }
}
