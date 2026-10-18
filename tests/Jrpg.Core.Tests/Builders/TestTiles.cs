using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Builders;

/// <summary>Tiles for tests that only care about walking; every one is cut from the same stand-in sheet.</summary>
internal static class TestTiles
{
    public static TilesheetDefinition Sheet { get; } = new()
    {
        Id = "test_sheet",
        File = "test_sheet.png",
        TileWidth = 48,
        TileHeight = 48,
        Margin = 0,
        Spacing = 0,
    };

    public static TileDefinition Walkable(int id) => new() { Id = id, IsWalkable = true, SheetId = Sheet.Id, Column = 0, Row = 0 };

    public static TileDefinition Blocking(int id) => Walkable(id) with { IsWalkable = false };
}
