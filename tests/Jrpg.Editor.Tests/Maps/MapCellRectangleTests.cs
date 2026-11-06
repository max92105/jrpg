using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Tests.Maps;

public sealed class MapCellRectangleTests
{
    private static readonly MapLayout _tenByEight = new() { Id = "glade", Width = 10, Height = 8, Layers = [], Objects = [] };

    [Fact]
    public void Between_TopLeftFirst_GivesTheRectangle()
    {
        Assert.Equal(new MapCellRectangle(new GridCell(2, 3), 4, 2), MapCellRectangle.Between(new GridCell(2, 3), new GridCell(5, 4)));
    }

    [Fact]
    public void Between_BottomRightFirst_GivesTheSameRectangle()
    {
        Assert.Equal(new MapCellRectangle(new GridCell(2, 3), 4, 2), MapCellRectangle.Between(new GridCell(5, 4), new GridCell(2, 3)));
    }

    [Fact]
    public void ClippedTo_RectanglePastTheRightAndBottom_KeepsTheInsidePart()
    {
        var clipped = MapCellRectangle.Between(new GridCell(8, 6), new GridCell(12, 11)).ClippedTo(_tenByEight);

        Assert.Equal(new MapCellRectangle(new GridCell(8, 6), 2, 2), clipped);
    }

    [Fact]
    public void ClippedTo_RectanglePastTheLeftAndTop_KeepsTheInsidePart()
    {
        var clipped = MapCellRectangle.Between(new GridCell(-2, -2), new GridCell(1, 0)).ClippedTo(_tenByEight);

        Assert.Equal(new MapCellRectangle(new GridCell(0, 0), 2, 1), clipped);
    }

    [Fact]
    public void ClippedTo_WhollyOutside_IsNull()
    {
        Assert.Null(MapCellRectangle.Between(new GridCell(11, 2), new GridCell(14, 3)).ClippedTo(_tenByEight));
    }

    [Fact]
    public void Cells_TwoByThree_ListsSixCellsRowByRow()
    {
        var cells = new MapCellRectangle(new GridCell(1, 1), 2, 3).Cells;

        Assert.Equal([new GridCell(1, 1), new GridCell(2, 1), new GridCell(1, 2), new GridCell(2, 2), new GridCell(1, 3), new GridCell(2, 3)], cells);
    }
}
