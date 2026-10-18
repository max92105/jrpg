using Jrpg.Core.Geometry;
using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class BrushCellsTests
{
    [Fact]
    public void Square_SizeThree_CoversNineCellsAroundTheCenter()
    {
        var cells = BrushCells.Square(new GridCell(5, 5), 3).ToList();

        Assert.Equal(9, cells.Distinct().Count());
        Assert.Equal((4, 6), (cells.Min(cell => cell.X), cells.Max(cell => cell.X)));
        Assert.Equal((4, 6), (cells.Min(cell => cell.Y), cells.Max(cell => cell.Y)));
    }

    [Fact]
    public void Square_EvenSize_ReachesFurtherRightAndDown()
    {
        Assert.Equal(
            [new GridCell(5, 5), new GridCell(6, 5), new GridCell(5, 6), new GridCell(6, 6)],
            BrushCells.Square(new GridCell(5, 5), 2));
    }

    [Fact]
    public void Line_DiagonalDrag_HasNoGaps()
    {
        var cells = BrushCells.Line(new GridCell(0, 0), new GridCell(5, 2)).ToList();

        Assert.Equal(new GridCell(0, 0), cells[0]);
        Assert.Equal(new GridCell(5, 2), cells[^1]);
        Assert.All(cells.Zip(cells.Skip(1)), pair =>
            Assert.Equal(1, Math.Max(Math.Abs(pair.Second.X - pair.First.X), Math.Abs(pair.Second.Y - pair.First.Y))));
    }

    [Fact]
    public void Rectangle_DraggedUpLeft_NormalizesCorners()
    {
        Assert.Equal(
            [new GridCell(1, 2), new GridCell(2, 2), new GridCell(1, 3), new GridCell(2, 3)],
            BrushCells.Rectangle(new GridCell(2, 3), new GridCell(1, 2)));
    }

    [Fact]
    public void Line_SameCell_ReturnsIt()
    {
        Assert.Equal([new GridCell(3, 4)], BrushCells.Line(new GridCell(3, 4), new GridCell(3, 4)));
    }
}
