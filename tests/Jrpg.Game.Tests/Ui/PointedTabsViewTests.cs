using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

// The tabs' layout (L2's arithmetic): each tab follows the one before it after the gap, centered in the row, and the
// tabs never run past the row (the review of L2: the Completed tab spilled over the detail column).
public sealed class PointedTabsViewTests
{
    [Fact]
    public void TabAt_TwoTabsInAWideRow_SecondStartsAfterTheFirstsWidthAndGap()
    {
        var row = new Rectangle(100, 200, 900, PointedTabsView.Height);

        var first = PointedTabsView.TabAt(row, 0, 2);
        var second = PointedTabsView.TabAt(row, 1, 2);

        Assert.Equal(row.X, first.X);
        Assert.Equal(first.Right + 24, second.X);
        Assert.Equal(PointedBarShape.CardHeaderWidth, first.Width);
        Assert.Equal(row.Y + ((PointedTabsView.Height - PointedBarShape.CardHeaderHeight) / 2), first.Y);
    }

    [Fact]
    public void TabAt_TwoTabsInTheQuestRow_LastEndsInsideTheRow()
    {
        var row = new Rectangle(100, 200, 584, PointedTabsView.Height);

        var first = PointedTabsView.TabAt(row, 0, 2);
        var second = PointedTabsView.TabAt(row, 1, 2);

        Assert.True(second.Right <= row.Right, "the last tab ends inside the row");
        Assert.Equal(first.Width, second.Width);
        Assert.Equal(first.Right + 24, second.X);
    }
}
