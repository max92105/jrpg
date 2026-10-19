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
}
