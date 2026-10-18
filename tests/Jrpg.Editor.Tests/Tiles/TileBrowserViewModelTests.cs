using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class TileBrowserViewModelTests
{
    // Two sheets: "dungeon" has 16-pixel cells inside a 2-pixel margin, 1 pixel apart, and one tile at column 1, row 0.
    private static readonly TilesheetDefinition _dungeon = new() { Id = "dungeon", File = "Dungeon.png", TileWidth = 16, TileHeight = 16, Margin = 2, Spacing = 1 };
    private static readonly TilesheetDefinition _forest = new() { Id = "forest", File = "Forest.png", TileWidth = 48, TileHeight = 48, Margin = 0, Spacing = 0 };

    private readonly TileBrowserViewModel _browser = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")) with
    {
        Tilesheets = new Dictionary<string, TilesheetDefinition> { ["forest"] = _forest, ["dungeon"] = _dungeon },
        Tiles = new Dictionary<int, TileDefinition>
        {
            [7] = new() { Id = 7, IsWalkable = true, SheetId = "dungeon", Column = 1, Row = 0 },
            [8] = new() { Id = 8, IsWalkable = true, SheetId = "forest", Column = 0, Row = 0 },
        },
    });

    [Fact]
    public void Constructor_SheetsSortedById_ShowsTheFirst()
    {
        Assert.Equal(["dungeon", "forest"], _browser.Sheets.Select(sheet => sheet.Id));
        Assert.Equal(_dungeon, _browser.SelectedSheet);
        Assert.Equal([7], _browser.SheetTiles.Select(tile => tile.Id));
        Assert.Null(_browser.SelectedTileId);
    }

    [Fact]
    public void SelectTileAt_PixelInATileCell_SelectsIt()
    {
        // Column 1 starts at margin 2 + one cell of 16 + spacing 1 = 19.
        _browser.SelectTileAt(19, 2);

        Assert.Equal(7, _browser.SelectedTileId);
    }

    [Fact]
    public void SelectTileAt_CellThatIsNoTile_KeepsTheSelection()
    {
        _browser.SelectTileAt(20, 5);

        _browser.SelectTileAt(5, 5);

        Assert.Equal(7, _browser.SelectedTileId);
    }

    [Fact]
    public void Select_TileOnAnotherSheet_ShowsItsSheet()
    {
        _browser.Select(8);

        Assert.Equal(_forest, _browser.SelectedSheet);
        Assert.Equal(8, _browser.SelectedTileId);
    }
}
