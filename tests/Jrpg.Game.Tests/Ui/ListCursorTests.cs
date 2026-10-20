using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class ListCursorTests
{
    [Fact]
    public void MoveDown_OnLastEntry_WrapsToFirst()
    {
        var cursor = new ListCursor(2, _ => true);
        cursor.MoveDown();

        cursor.MoveDown();

        Assert.Equal(0, cursor.Selected);
    }

    [Fact]
    public void MoveUp_OnFirstEntry_WrapsToLast()
    {
        var cursor = new ListCursor(3, _ => true);

        cursor.MoveUp();

        Assert.Equal(2, cursor.Selected);
    }

    [Fact]
    public void MoveDown_SkipsDisabledEntry()
    {
        var cursor = new ListCursor(3, index => index != 1);

        cursor.MoveDown();

        Assert.Equal(2, cursor.Selected);
    }

    [Fact]
    public void Constructor_FirstEntryDisabled_SelectsFirstEnabled()
    {
        var cursor = new ListCursor(3, index => index != 0);

        Assert.Equal(1, cursor.Selected);
    }

    [Fact]
    public void Constructor_NoEntryEnabled_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ListCursor(2, _ => false));
    }

    [Fact]
    public void MoveDown_PastVisibleRows_ScrollsWindow()
    {
        var cursor = new ListCursor(20, _ => true, visibleRows: 5);
        for (var press = 0; press < 5; press++)
        {
            cursor.MoveDown();
        }

        Assert.Equal(5, cursor.Selected);
        Assert.Equal(1, cursor.FirstVisible);
    }

    [Fact]
    public void MoveUp_OnFirstEntry_WrapsToLastAndScrolls()
    {
        var cursor = new ListCursor(20, _ => true, visibleRows: 5);

        cursor.MoveUp();

        Assert.Equal(19, cursor.Selected);
        Assert.Equal(15, cursor.FirstVisible);
    }

    [Fact]
    public void MoveDown_WithinVisibleRows_KeepsWindow()
    {
        var cursor = new ListCursor(20, _ => true, visibleRows: 5);

        cursor.MoveDown();

        Assert.Equal(0, cursor.FirstVisible);
    }
}
