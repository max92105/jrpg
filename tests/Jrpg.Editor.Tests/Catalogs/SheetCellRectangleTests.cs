using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

public sealed class SheetCellRectangleTests
{
    [Fact]
    public void Between_CornersInAnyOrder_GivesTheSameRectangle()
    {
        var rectangle = SheetCellRectangle.Between(new SheetCell("forest", 5, 7), new SheetCell("forest", 3, 4));

        Assert.Equal(new SheetCellRectangle("forest", 3, 4, 3, 4), rectangle);
        Assert.Equal(rectangle, SheetCellRectangle.Between(new SheetCell("forest", 3, 7), new SheetCell("forest", 5, 4)));
    }

    [Fact]
    public void Cells_TwoByTwo_ListsRowByRow()
    {
        var rectangle = new SheetCellRectangle("forest", 1, 2, 2, 2);

        Assert.Equal(
            [new SheetCell("forest", 1, 2), new SheetCell("forest", 2, 2), new SheetCell("forest", 1, 3), new SheetCell("forest", 2, 3)],
            rectangle.Cells);
    }
}
