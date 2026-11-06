using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects.Composing;

namespace Jrpg.Editor.Tests.Objects.Composing;

public sealed class ObjectCanvasTests
{
    // Cells (4, 1) to (5, 2) of forest.
    private static readonly SheetCellRectangle _square = new("forest", 4, 1, 2, 2);

    [Fact]
    public void Of_Rectangle_PaintsEachCellAtItsPlace()
    {
        var canvas = ObjectCanvas.Of(new SheetCellRectangle("forest", 4, 1, 3, 2));

        Assert.Equal((3, 2), (canvas.Width, canvas.Height));
        Assert.Equal(6, canvas.Cells.Count);
        Assert.Equal(new SheetCell("forest", 4, 1), canvas.Cells[new CellOnLevel(new GridCell(0, 0), 0)]);
        Assert.Equal(new SheetCell("forest", 6, 2), canvas.Cells[new CellOnLevel(new GridCell(2, 1), 0)]);
    }

    [Fact]
    public void Of_Object_ReadsEachTileBackToItsSheetCell()
    {
        var tiles = new Dictionary<int, TileDefinition>
        {
            [7] = new() { Id = 7, IsWalkable = true, SheetId = "forest", Column = 3, Row = 5 },
            [9] = new() { Id = 9, IsWalkable = false, SheetId = "house", Column = 0, Row = 2 },
        };
        var definition = new ObjectDefinition
        {
            Id = "well",
            Category = "General",
            Width = 2,
            Height = 3,
            Tiles = [new ObjectTile(1, 0, 7), new ObjectTile(0, 2, 9)],
            CollisionPolygon = [],
            AbovePlayerZones = [],
        };

        var canvas = ObjectCanvas.Of(definition, tiles);

        Assert.Equal((2, 3), (canvas.Width, canvas.Height));
        Assert.Equal(
            new Dictionary<CellOnLevel, SheetCell> { [new CellOnLevel(new GridCell(1, 0), 0)] = new("forest", 3, 5), [new CellOnLevel(new GridCell(0, 2), 0)] = new("house", 0, 2) },
            canvas.Cells);
    }

    [Fact]
    public void Stamp_RectangleAtACell_PaintsItsCellsFromThere()
    {
        var canvas = ObjectCanvas.Empty(4, 4).Stamp(0, new GridCell(1, 2), _square);

        Assert.Equal(
            new Dictionary<CellOnLevel, SheetCell>
            {
                [new CellOnLevel(new GridCell(1, 2), 0)] = new("forest", 4, 1),
                [new CellOnLevel(new GridCell(2, 2), 0)] = new("forest", 5, 1),
                [new CellOnLevel(new GridCell(1, 3), 0)] = new("forest", 4, 2),
                [new CellOnLevel(new GridCell(2, 3), 0)] = new("forest", 5, 2),
            },
            canvas.Cells);
    }

    [Fact]
    public void Stamp_PastTheRightAndBottomEdges_KeepsOnlyTheCellsInside()
    {
        var canvas = ObjectCanvas.Empty(3, 3).Stamp(0, new GridCell(2, 2), _square);

        Assert.Equal(new Dictionary<CellOnLevel, SheetCell> { [new CellOnLevel(new GridCell(2, 2), 0)] = new("forest", 4, 1) }, canvas.Cells);
    }

    [Fact]
    public void Stamp_OverAPaintedCell_ReplacesIt()
    {
        var canvas = ObjectCanvas.Of(_square).Stamp(0, new GridCell(1, 1), new SheetCellRectangle("house", 7, 8, 1, 1));

        Assert.Equal(new SheetCell("house", 7, 8), canvas.Cells[new CellOnLevel(new GridCell(1, 1), 0)]);
        Assert.Equal(4, canvas.Cells.Count);
    }

    [Fact]
    public void Erase_PaintedCell_LeavesItEmpty()
    {
        var canvas = ObjectCanvas.Of(_square).Erase(0, new GridCell(0, 1));

        Assert.False(canvas.Cells.ContainsKey(new CellOnLevel(new GridCell(0, 1), 0)));
        Assert.Equal(3, canvas.Cells.Count);
    }

    [Fact]
    public void Resize_Smaller_DropsTheCellsOutside()
    {
        var canvas = ObjectCanvas.Of(_square).Resize(1, 2);

        Assert.Equal((1, 2), (canvas.Width, canvas.Height));
        Assert.Equal(
            new Dictionary<CellOnLevel, SheetCell> { [new CellOnLevel(new GridCell(0, 0), 0)] = new("forest", 4, 1), [new CellOnLevel(new GridCell(0, 1), 0)] = new("forest", 4, 2) },
            canvas.Cells);
    }

    [Fact]
    public void Resize_Larger_KeepsTheCellsWhereTheyWere()
    {
        var canvas = ObjectCanvas.Of(_square).Resize(5, 3);

        Assert.Equal((5, 3), (canvas.Width, canvas.Height));
        Assert.Equal(ObjectCanvas.Of(_square).Cells, canvas.Cells);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(ObjectCanvas.MaxCells + 1, 1)]
    [InlineData(1, ObjectCanvas.MaxCells + 1)]
    public void Resize_OutsideOneToMaxCells_Throws(int width, int height) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ObjectCanvas.Of(_square).Resize(width, height));

    [Fact]
    public void IsBlank_Empty_IsTrue() => Assert.True(ObjectCanvas.Empty(3, 3).IsBlank);

    [Fact]
    public void IsBlank_OneCellPainted_IsFalse() =>
        Assert.False(ObjectCanvas.Empty(3, 3).Stamp(0, new GridCell(1, 1), new SheetCellRectangle("forest", 0, 0, 1, 1)).IsBlank);

    [Fact]
    public void TilesOf_ScatteredCells_OneTilePerPaintedCellRowByRow()
    {
        var canvas = ObjectCanvas.Empty(3, 3)
            .Stamp(0, new GridCell(2, 2), new SheetCellRectangle("forest", 1, 0, 1, 1))
            .Stamp(0, new GridCell(2, 0), new SheetCellRectangle("forest", 2, 0, 1, 1))
            .Stamp(0, new GridCell(0, 1), new SheetCellRectangle("house", 3, 0, 1, 1));

        var tiles = canvas.TilesOf(cell => (cell.SheetId == "house" ? 100 : 0) + cell.Column);

        Assert.Equal([new ObjectTile(2, 0, 2), new ObjectTile(0, 1, 103), new ObjectTile(2, 2, 1)], tiles);
    }

    [Fact]
    public void OfCells_CellsInside_HoldsThem()
    {
        var cell = new CellOnLevel(new GridCell(1, 2), -1);

        var canvas = ObjectCanvas.OfCells(2, 3, new Dictionary<CellOnLevel, SheetCell> { [cell] = new("trees", 3, 4) });

        Assert.Equal((2, 3), (canvas.Width, canvas.Height));
        Assert.Equal(new SheetCell("trees", 3, 4), Assert.Single(canvas.Cells).Value);
    }

    [Fact]
    public void OfCells_CellOutsideTheSize_Throws()
    {
        var cells = new Dictionary<CellOnLevel, SheetCell> { [new CellOnLevel(new GridCell(2, 0), 0)] = new("trees", 0, 0) };

        Assert.Throws<ArgumentOutOfRangeException>(() => ObjectCanvas.OfCells(2, 3, cells));
    }

    [Fact]
    public void OfCells_LevelOutsideTheRange_Throws()
    {
        var cells = new Dictionary<CellOnLevel, SheetCell> { [new CellOnLevel(new GridCell(0, 0), ObjectTile.MaxLevel + 1)] = new("trees", 0, 0) };

        Assert.Throws<ArgumentOutOfRangeException>(() => ObjectCanvas.OfCells(2, 3, cells));
    }
}
