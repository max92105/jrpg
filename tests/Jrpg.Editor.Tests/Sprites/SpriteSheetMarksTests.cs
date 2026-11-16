using System.Windows;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

// A 64 x 64 image of 16 px rows shown at twice its size: a row's number is set at 12 screen pixels (6 image pixels),
// drawn 1 image pixel in from the left edge and half a pixel below the row's top.
public sealed class SpriteSheetMarksTests
{
    private const int ImageHeight = 64;
    private const int FrameHeight = 16;
    private const double Scale = 2;

    // Rows count from 0 in code: the number drawn there reads 2.
    [Fact]
    public void RowAt_PointOnRowTwosNumber_IsRowTwo()
    {
        var row = SpriteSheetMarks.RowAt(new Point(2, FrameHeight + 3), ImageHeight, FrameHeight, Scale);

        Assert.Equal(1, row);
    }

    // A one-digit number is about 3.3 image pixels wide from its start at 1: a point 6 pixels in lies beside it.
    [Fact]
    public void RowAt_PointJustRightOfTheNumber_IsNull()
    {
        var row = SpriteSheetMarks.RowAt(new Point(6, FrameHeight + 3), ImageHeight, FrameHeight, Scale);

        Assert.Null(row);
    }

    // A 70 px image holds four whole rows; the 6 px left below them draws no number, so its place picks nothing.
    [Fact]
    public void RowAt_PointWhereAPartialRowsNumberWouldBe_IsNull()
    {
        var row = SpriteSheetMarks.RowAt(new Point(2, (4 * FrameHeight) + 3), 70, FrameHeight, Scale);

        Assert.Null(row);
    }

    [Fact]
    public void RowAt_PointInsideACell_IsNull()
    {
        var row = SpriteSheetMarks.RowAt(new Point(10, FrameHeight + 8), ImageHeight, FrameHeight, Scale);

        Assert.Null(row);
    }
}
