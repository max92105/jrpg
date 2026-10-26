using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class SelectionMotionTests
{
    private const int EntryCount = 10;
    private const int VisibleRows = 3;

    [Fact]
    public void Follow_AScrollThatKeepsTheRowInPlace_StaysOnIt()
    {
        var cursor = new ListCursor(EntryCount, _ => true, VisibleRows);
        cursor.Select(VisibleRows - 1);
        var motion = new SelectionMotion();
        motion.JumpTo(cursor);

        cursor.MoveDown();
        motion.Follow(cursor);
        motion.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(VisibleRows - 1, motion.Position);
    }

    [Fact]
    public void Follow_AWrapToTheTop_JumpsThere()
    {
        var cursor = new ListCursor(EntryCount, _ => true, VisibleRows);
        cursor.Select(EntryCount - 1);
        var motion = new SelectionMotion();
        motion.JumpTo(cursor);

        cursor.MoveDown();
        motion.Follow(cursor);

        Assert.Equal(0, motion.Position);
    }

    [Fact]
    public void RowIn_AfterGlidingToTheSecondRow_IsOneRowDown()
    {
        var cursor = new ListCursor(EntryCount, _ => true, VisibleRows);
        var motion = new SelectionMotion();
        cursor.MoveDown();
        motion.Follow(cursor);

        motion.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(new Rectangle(10, 150, 300, 50), motion.RowIn(new Rectangle(10, 100, 300, 50)).Area);
    }

    [Fact]
    public void PointerOffset_HalfwayThroughTheBob_IsTheFullDistance()
    {
        var motion = new SelectionMotion();

        motion.Advance(TimeSpan.FromSeconds(0.45));

        Assert.Equal(MenuPointer.BobDistance, motion.PointerOffset, 3);
    }
}
