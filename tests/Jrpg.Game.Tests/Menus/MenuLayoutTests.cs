using Jrpg.Game.Menus;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Menus;

public sealed class MenuLayoutTests
{
    // The area the pages had before MenuLayout: 24 px right of the 410 px tabs and 40 px from the screen's right,
    // between 40 px under the header and 32 px over the footer.
    [Fact]
    public void PageArea_BesideTheTabs_IsBetweenTheMargins()
    {
        var area = MenuLayout.PageArea(tabsRight: 410);

        Assert.Equal(new Rectangle(434, 128, 1446, 848), area);
    }

    [Fact]
    public void PageArea_NarrowerTabs_StartsFurtherLeft()
    {
        var beside = MenuLayout.PageArea(tabsRight: 410);

        var besideFolded = MenuLayout.PageArea(tabsRight: 120);

        Assert.Equal(beside.X - 290, besideFolded.X);
    }

    [Fact]
    public void PageArea_NarrowerTabs_KeepsItsRightEdge()
    {
        var beside = MenuLayout.PageArea(tabsRight: 410);

        var besideFolded = MenuLayout.PageArea(tabsRight: 120);

        Assert.Equal(beside.Right, besideFolded.Right);
        Assert.Equal((beside.Y, beside.Height), (besideFolded.Y, besideFolded.Height));
    }

    [Fact]
    public void SingleColumn_WiderArea_IsCappedAtTheColumnWidth()
    {
        var area = new Rectangle(434, 128, 1446, 848);

        var column = MenuLayout.SingleColumn(area);

        Assert.Equal(new Rectangle(434, 128, MenuLayout.SingleColumnWidth, 848), column);
    }

    [Fact]
    public void SingleColumn_NarrowerArea_IsUnchanged()
    {
        var area = new Rectangle(434, 128, 900, 848);

        var column = MenuLayout.SingleColumn(area);

        Assert.Equal(area, column);
    }
}
