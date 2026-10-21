using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class WalkabilityMarkingTests
{
    // One 16-pixel sheet, 48 x 16: three cells in a row, a blocked tile 7 in the middle one.
    private static readonly TilesheetDefinition _sheet = new() { Id = "dungeon", File = "Dungeon.png", TileWidth = 16, TileHeight = 16, Margin = 0, Spacing = 0 };

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content")) with
    {
        Tilesheets = new Dictionary<string, TilesheetDefinition> { ["dungeon"] = _sheet },
        Tiles = new Dictionary<int, TileDefinition>
        {
            [7] = new() { Id = 7, IsWalkable = false, SheetId = "dungeon", Column = 1, Row = 0 },
        },
    });

    private readonly WalkabilityMarking _marking;

    public WalkabilityMarkingTests()
    {
        _marking = new WalkabilityMarking(_draft, new TileBrowserViewModel(_draft, ["Dungeon.png"], _ => (48, 16)));
    }

    [Fact]
    public void FlipCellAt_CellNotATile_AddsBlockedTile()
    {
        _marking.FlipCellAt(40, 5);

        Assert.Equal(
            new TileDefinition { Id = 8, IsWalkable = false, SheetId = "dungeon", Column = 2, Row = 0 },
            _draft.TileAt(new SheetCell("dungeon", 2, 0)));
    }

    [Fact]
    public void FlipCellAt_BlockedTile_MakesItWalkable()
    {
        _marking.FlipCellAt(20, 5);

        Assert.True(_draft.Current.Tiles[7].IsWalkable);
        Assert.Single(_draft.Current.Tiles);
    }

    [Fact]
    public void FlipCellAt_OutsideWholeCells_ChangesNothing()
    {
        _marking.FlipCellAt(50, 5);

        Assert.False(_draft.IsDirty);
    }
}
