using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects.Composing;

namespace Jrpg.Editor.Tests.Objects.Composing;

// The canvas's levels (ADR 0040): a wall cell on the base, a window over it on level 1.
public sealed class ObjectCanvasLevelTests
{
    private static readonly SheetCellRectangle _wall = new("house", 0, 0, 1, 1);
    private static readonly SheetCellRectangle _window = new("house", 3, 2, 1, 1);
    private static readonly GridCell _cell = new(1, 1);

    [Fact]
    public void Stamp_OnLevelOne_KeepsTheBaseCellUnder()
    {
        var canvas = ObjectCanvas.Empty(3, 3).Stamp(0, _cell, _wall).Stamp(1, _cell, _window);

        Assert.Equal(new SheetCell("house", 0, 0), canvas.Cells[new CellOnLevel(_cell, 0)]);
        Assert.Equal(new SheetCell("house", 3, 2), canvas.Cells[new CellOnLevel(_cell, 1)]);
    }

    [Fact]
    public void Erase_OnLevelOne_LeavesTheBaseCell()
    {
        var canvas = ObjectCanvas.Empty(3, 3).Stamp(0, _cell, _wall).Stamp(1, _cell, _window).Erase(1, _cell);

        Assert.Equal([new CellOnLevel(_cell, 0)], canvas.Cells.Keys);
    }

    [Theory]
    [InlineData(ObjectTile.MinLevel - 1)]
    [InlineData(ObjectTile.MaxLevel + 1)]
    public void Stamp_OutsideTheLevels_Throws(int level) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ObjectCanvas.Empty(3, 3).Stamp(level, _cell, _wall));

    [Theory]
    [InlineData(ObjectTile.MinLevel)]
    [InlineData(ObjectTile.MaxLevel)]
    public void Stamp_AtEachBound_PaintsIt(int level) =>
        Assert.Equal([new CellOnLevel(_cell, level)], ObjectCanvas.Empty(3, 3).Stamp(level, _cell, _wall).Cells.Keys);

    [Fact]
    public void Stamp_UnderTheBase_KeepsTheBaseCellOver()
    {
        var canvas = ObjectCanvas.Empty(3, 3).Stamp(0, _cell, _wall).Stamp(-1, _cell, _window);

        Assert.Equal(new SheetCell("house", 0, 0), canvas.Cells[new CellOnLevel(_cell, 0)]);
        Assert.Equal(new SheetCell("house", 3, 2), canvas.Cells[new CellOnLevel(_cell, -1)]);
    }

    // The base is stamped first, the opposite of the expected order.
    [Fact]
    public void TilesOf_LevelUnderTheBase_ListsItFirst()
    {
        var canvas = ObjectCanvas.Empty(3, 3).Stamp(0, _cell, _wall).Stamp(-1, _cell, _window);

        Assert.Equal([new ObjectTile(1, 1, 3, Level: -1), new ObjectTile(1, 1, 0)], canvas.TilesOf(cell => cell.Column));
    }

    [Fact]
    public void HasCellsOn_PaintedLevel_IsTrue() => Assert.True(ObjectCanvas.Empty(3, 3).Stamp(-1, _cell, _wall).HasCellsOn(-1));

    [Fact]
    public void HasCellsOn_EmptyLevel_IsFalse() => Assert.False(ObjectCanvas.Empty(3, 3).Stamp(0, _cell, _wall).HasCellsOn(1));

    [Fact]
    public void Of_StackedObject_PutsEachTileOnItsLevel()
    {
        var tiles = new Dictionary<int, TileDefinition>
        {
            [7] = new() { Id = 7, IsWalkable = true, SheetId = "house", Column = 0, Row = 0 },
            [9] = new() { Id = 9, IsWalkable = true, SheetId = "house", Column = 3, Row = 2 },
        };
        var definition = new ObjectDefinition
        {
            Id = "house_wall",
            Category = "House",
            Width = 2,
            Height = 2,
            Tiles = [new ObjectTile(1, 1, 7), new ObjectTile(1, 1, 9, Level: 2)],
            CollisionPolygon = [],
            AbovePlayerZones = [],
        };

        var canvas = ObjectCanvas.Of(definition, tiles);

        Assert.Equal(
            new Dictionary<CellOnLevel, SheetCell> { [new CellOnLevel(_cell, 0)] = new("house", 0, 0), [new CellOnLevel(_cell, 2)] = new("house", 3, 2) },
            canvas.Cells);
    }

    [Fact]
    public void TilesOf_TwoLevels_ListsTheBaseLevelFirstWithTheirLevels()
    {
        var canvas = ObjectCanvas.Empty(3, 3).Stamp(1, new GridCell(0, 0), _window).Stamp(0, new GridCell(2, 2), _wall).Stamp(0, _cell, _wall);

        var tiles = canvas.TilesOf(cell => cell.Column);

        Assert.Equal([new ObjectTile(1, 1, 0), new ObjectTile(2, 2, 0), new ObjectTile(0, 0, 3, Level: 1)], tiles);
    }
}
