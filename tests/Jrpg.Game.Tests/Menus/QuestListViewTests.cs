using Jrpg.Game.Menus;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

// The list column's arithmetic (L1): the body less the top margin and the tabs' room holds fourteen rows of 52 px.
public sealed class QuestListViewTests
{
    [Fact]
    public void VisibleRows_BodyWithTabs_IsFourteen()
    {
        var used = PageListLayout.TopMargin + QuestListView.TabsReserve + (QuestListView.VisibleRows * PageListLayout.RowHeight);

        Assert.Equal(14, QuestListView.VisibleRows);
        Assert.True(used <= MenuLayout.BodyHeight);
        Assert.True(used + PageListLayout.RowHeight > MenuLayout.BodyHeight, "one more row would fit");
    }
}
