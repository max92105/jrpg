using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Rendering;

public sealed class ObjectSpriteBakerTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void CanopyPixels_TopHalfZone_CopiesTopRowsAndClearsTheRest()
    {
        // A 2x2 sheet cell scaled to a 4-pixel cell: each sheet pixel covers 2x2 canopy pixels.
        var (catalog, sheet) = OneTileCatalog(cellWidth: 2);
        var definition = catalog.Objects["rock"];

        var pixels = ObjectSpriteBaker.CanopyPixels(definition, new TileAtlas(catalog), _ => sheet, cellPixels: 4);

        Assert.Equal(Color.Red, pixels[0]);
        Assert.Equal(Color.Blue, pixels[3]);
        Assert.Equal(Color.Blue, pixels[(1 * 4) + 3]);
        Assert.Equal(Color.Transparent, pixels[(2 * 4) + 0]);
        Assert.Equal(Color.Transparent, pixels[(3 * 4) + 3]);
    }

    [Fact]
    public void CanopyPixels_RepositoryTree_HasSomeCanopyAndSomeClearPixels()
    {
        var tree = _catalog.Objects["tree"];

        var pixels = ObjectSpriteBaker.CanopyPixels(tree, new TileAtlas(_catalog), file => Opaque(_catalog, file), cellPixels: 48);

        Assert.Equal(tree.Width * 48 * tree.Height * 48, pixels.Length);
        Assert.Contains(Color.White, pixels);
        Assert.Contains(Color.Transparent, pixels);
    }

    // Every tilesheet as solid white, big enough for any cell; the canopy then shows exactly where the mask is.
    private static SheetPixels Opaque(ContentCatalog catalog, string file)
    {
        var sheet = catalog.Tilesheets.Values.First(sheet => sheet.File == file);
        var width = sheet.CellLeft(64);
        return new SheetPixels([.. Enumerable.Repeat(Color.White, width * sheet.CellTop(64))], width);
    }

    private static (ContentCatalog Catalog, SheetPixels Sheet) OneTileCatalog(int cellWidth)
    {
        var sheetDefinition = new TilesheetDefinition { Id = "rocks", File = "Rocks.png", TileWidth = cellWidth, TileHeight = cellWidth, Margin = 0, Spacing = 0 };
        var rock = new ObjectDefinition
        {
            Id = "rock",
            Width = 1,
            Height = 1,
            CollisionPolygon = [],
            Category = "General",
            Tiles = [new ObjectTile(0, 0, 1)],
            AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 0.5), new Point2D(0, 0.5)]],
        };
        var catalog = _catalog with
        {
            Tilesheets = new Dictionary<string, TilesheetDefinition> { ["rocks"] = sheetDefinition },
            Tiles = new Dictionary<int, TileDefinition> { [1] = new() { Id = 1, IsWalkable = true, SheetId = "rocks", Column = 0, Row = 0 } },
            Objects = new Dictionary<string, ObjectDefinition> { ["rock"] = rock },
        };

        // Top row red then blue, bottom row green.
        return (catalog, new SheetPixels([Color.Red, Color.Blue, Color.Green, Color.Green], 2));
    }
}
