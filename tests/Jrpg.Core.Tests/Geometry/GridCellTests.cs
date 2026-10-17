using Jrpg.Core.Geometry;

namespace Jrpg.Core.Tests.Geometry;

public sealed class GridCellTests
{
    [Fact]
    public void Containing_NegativeFraction_RoundsDown()
    {
        var cell = GridCell.Containing(new Point2D(-0.25, -1.5));

        Assert.Equal(new GridCell(-1, -2), cell);
    }

    [Fact]
    public void Containing_PointInsideCell_ReturnsThatCell()
    {
        var cell = GridCell.Containing(new Point2D(2.99, 3.5));

        Assert.Equal(new GridCell(2, 3), cell);
    }

    [Fact]
    public void Center_Cell_IsHalfATileInOnBothAxes()
    {
        var center = new GridCell(2, -1).Center;

        Assert.Equal(new Point2D(2.5, -0.5), center);
    }

    [Fact]
    public void Containing_PointOnTopLeftCorner_BelongsToThatCell()
    {
        var cell = GridCell.Containing(new Point2D(2.0, 3.0));

        Assert.Equal(new GridCell(2, 3), cell);
    }
}
