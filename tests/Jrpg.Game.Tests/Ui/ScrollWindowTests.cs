using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class ScrollWindowTests
{
    private const int Count = 10;
    private const int Visible = 4;

    [Fact]
    public void KeepInView_IndexInView_KeepsTheWindow()
    {
        Assert.Equal(2, ScrollWindow.KeepInView(firstVisible: 2, index: 5, Count, Visible));
    }

    [Fact]
    public void KeepInView_IndexAboveTheWindow_StartsAtIt()
    {
        Assert.Equal(1, ScrollWindow.KeepInView(firstVisible: 3, index: 1, Count, Visible));
    }

    [Fact]
    public void KeepInView_IndexBelowTheWindow_EndsAtIt()
    {
        Assert.Equal(4, ScrollWindow.KeepInView(firstVisible: 2, index: 7, Count, Visible));
    }

    [Fact]
    public void Clamp_ListGotShorter_EndsTheWindowAtTheLastEntry()
    {
        Assert.Equal(1, ScrollWindow.Clamp(firstVisible: 6, count: 5, Visible));
    }

    [Fact]
    public void Clamp_ListShorterThanTheWindow_StartsAtTheTop()
    {
        Assert.Equal(0, ScrollWindow.Clamp(firstVisible: 2, count: 3, Visible));
    }
}
