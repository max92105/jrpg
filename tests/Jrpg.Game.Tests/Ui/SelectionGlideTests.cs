using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class SelectionGlideTests
{
    private static readonly TimeSpan _glide = TimeSpan.FromSeconds(0.08);
    private const int RowCount = 6;

    [Fact]
    public void Position_AtStart_IsTheFirstSelectedRow()
    {
        var glide = new SelectionGlide(2);

        Assert.Equal(2, glide.Position);
    }

    [Fact]
    public void Position_RightAfterSelect_IsThePreviousRow()
    {
        var glide = new SelectionGlide(0);

        glide.Select(1);

        Assert.Equal(0, glide.Position);
    }

    [Fact]
    public void Position_AfterTheGlide_IsTheSelectedRow()
    {
        var glide = new SelectionGlide(0);
        glide.Select(1);

        glide.Advance(_glide);

        Assert.Equal(1, glide.Position);
    }

    [Fact]
    public void Position_HalfwayThroughTheGlide_IsBetweenTheRows()
    {
        var glide = new SelectionGlide(0);
        glide.Select(1);

        glide.Advance(_glide / 2);

        Assert.InRange(glide.Position, 0.01f, 0.99f);
    }

    [Fact]
    public void Select_DuringAGlide_StartsFromTheCurrentPosition()
    {
        var glide = new SelectionGlide(0);
        glide.Select(1);
        glide.Advance(_glide / 2);
        var midway = glide.Position;

        glide.Select(2);

        Assert.Equal(midway, glide.Position);
    }

    [Fact]
    public void JumpTo_DuringAGlide_IsAtTheRowAtOnce()
    {
        var glide = new SelectionGlide(0);
        glide.Select(1);
        glide.Advance(_glide / 2);

        glide.JumpTo(5);

        Assert.Equal(5, glide.Position);
    }

    [Fact]
    public void Follow_NeighbouringRow_Glides()
    {
        var glide = new SelectionGlide(0);

        glide.Follow(1, RowCount);

        Assert.Equal(0, glide.Position);
    }

    [Fact]
    public void Follow_WrapAroundTheList_Jumps()
    {
        var glide = new SelectionGlide(0);

        glide.Follow(5, RowCount);

        Assert.Equal(5, glide.Position);
    }
    [Fact]
    public void Follow_SkippingADisabledRow_Glides()
    {
        var glide = new SelectionGlide(0);

        glide.Follow(2, RowCount);

        Assert.Equal(0, glide.Position);
    }

    [Fact]
    public void Follow_SameRowDuringAGlide_KeepsGliding()
    {
        var glide = new SelectionGlide(0);
        glide.Select(1);
        glide.Advance(_glide / 2);

        glide.Follow(1, RowCount);
        glide.Advance(_glide / 2);

        Assert.Equal(1, glide.Position);
    }
}
