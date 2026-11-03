using System.Windows.Media.Imaging;
using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

/// <summary>What the drawing gets for art the folder lacks (decision 442): the mark, never cached.</summary>
public sealed class TileImagesTests
{
    private static readonly string _root = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_root);

    private static readonly MissingArt _cityMissing = MissingArt.Of(_catalog, file => file != "City.png", _ => true);
    private static readonly MissingArt _spriteMissing = MissingArt.Of(_catalog, _ => true, _ => false);

    private static int CityTileId => _catalog.Tiles.Values.First(tile => tile.SheetId == "city").Id;

    [Fact]
    public void TileImageOf_TileOfMissingSheet_IsTheMark()
    {
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => _cityMissing);

        Assert.Same(MissingArtMark.Picture, images.TileImageOf(CityTileId));
    }

    [Fact]
    public void TileImageOf_SheetBackAfterTheMark_IsTheTile()
    {
        var missing = _cityMissing;
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => missing);
        Assert.Same(MissingArtMark.Picture, images.TileImageOf(CityTileId));

        missing = MissingArt.None;

        Assert.IsType<CroppedBitmap>(images.TileImageOf(CityTileId));
    }

    // Grass1.png is 48 x 48, one cell of the city sheet's 48 px cut: every city tile past cell (0, 0) lies outside it.
    [Fact]
    public void TileImageOf_CellPastASmallerImage_IsTheMark()
    {
        var city = _catalog.Tilesheets["city"] with { File = "Grass1.png" };
        var catalog = _catalog with { Tilesheets = new Dictionary<string, TilesheetDefinition>(_catalog.Tilesheets) { ["city"] = city } };
        var outside = catalog.Tiles.Values.First(tile => tile.SheetId == "city" && (tile.Column > 0 || tile.Row > 0));
        var images = new TileImages(new CatalogDraft(catalog), ArtFolders.RootIn(_root), () => MissingArt.None);

        Assert.Same(MissingArtMark.Picture, images.TileImageOf(outside.Id));
    }

    [Fact]
    public void FieldSpriteFrameOf_MissingSprite_IsTheMark()
    {
        var images = new TileImages(new CatalogDraft(_catalog), ArtFolders.RootIn(_root), () => _spriteMissing);

        Assert.Same(MissingArtMark.Picture, images.FieldSpriteFrameOf("npc_placeholder.png"));
    }
}
