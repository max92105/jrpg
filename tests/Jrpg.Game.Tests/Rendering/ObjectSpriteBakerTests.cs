using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Rendering;

public sealed class ObjectSpriteBakerTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

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
    public void CanopyPixels_TestWorldTree_HasSomeCanopyAndSomeClearPixels()
    {
        var tree = _catalog.Objects["tree"];

        var pixels = ObjectSpriteBaker.CanopyPixels(tree, new TileAtlas(_catalog), file => Opaque(_catalog, file), cellPixels: 48);

        Assert.Equal(tree.Width * 48 * tree.Height * 48, pixels.Length);
        Assert.Contains(Color.White, pixels);
        Assert.Contains(Color.Transparent, pixels);
    }

    [Fact]
    public void CanopyPixels_TransparentPixelOnTheUpperLevel_KeepsTheBaseTilesPixel()
    {
        var (definition, atlas, sheet) = StackedObject(upper: Color.Transparent);

        var pixels = ObjectSpriteBaker.CanopyPixels(definition, atlas, _ => sheet, cellPixels: 1);

        Assert.Equal(Color.Red, pixels[0]);
    }

    [Fact]
    public void CanopyPixels_OpaquePixelOnTheUpperLevel_CoversTheBaseTile()
    {
        var (definition, atlas, sheet) = StackedObject(upper: Color.Blue);

        var pixels = ObjectSpriteBaker.CanopyPixels(definition, atlas, _ => sheet, cellPixels: 1);

        Assert.Equal(Color.Blue, pixels[0]);
    }

    [Fact]
    public void CanopyPixels_HalfTransparentPixelOnTheUpperLevel_BlendsOverTheBaseTile()
    {
        // Premultiplied half-alpha grey over opaque red 200: 128 + 200 x (1 - 128 / 255) = 227 red, 128 green and blue.
        var (definition, atlas, sheet) = StackedObject(upper: new Color(128, 128, 128, 128), lower: new Color(200, 0, 0, 255));

        var pixels = ObjectSpriteBaker.CanopyPixels(definition, atlas, _ => sheet, cellPixels: 1);

        Assert.Equal((227, 128, 128), (pixels[0].R, pixels[0].G, pixels[0].B));
        Assert.InRange(pixels[0].A, 254, 255);
    }

    // One wholly zoned cell: the sheet's left pixel is tile 1, on the base; its right pixel is tile 2, on level 1.
    private static (ObjectDefinition Definition, TileAtlas Atlas, SheetPixels Sheet) StackedObject(Color upper, Color? lower = null)
    {
        var (catalog, _) = OneTileCatalog(cellWidth: 1);
        var definition = catalog.Objects["rock"] with
        {
            Tiles = [new ObjectTile(0, 0, 1), new ObjectTile(0, 0, 2, Level: 1)],
            AbovePlayerZones = [[new Point2D(0, 0), new Point2D(1, 0), new Point2D(1, 1), new Point2D(0, 1)]],
        };
        var tiles = new Dictionary<int, TileDefinition>(catalog.Tiles) { [2] = catalog.Tiles[1] with { Id = 2, Column = 1 } };
        return (definition, new TileAtlas(catalog with { Tiles = tiles }), new SheetPixels([lower ?? Color.Red, upper], 2));
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
