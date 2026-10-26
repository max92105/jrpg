using Jrpg.Game.Title;

namespace Jrpg.Game.Tests.Title;

public sealed class TitleEntranceTests
{
    [Fact]
    public void PictureOpacity_AtOpening_IsZero()
    {
        var entrance = new TitleEntrance();

        entrance.MarkDrawn();

        Assert.Equal(0, entrance.PictureOpacity);
    }

    [Fact]
    public void PictureOpacity_HalfwayThroughItsFade_IsBetweenZeroAndOne()
    {
        var entrance = new TitleEntrance();
        entrance.MarkDrawn();

        entrance.Advance(TimeSpan.FromSeconds(0.3));

        Assert.InRange(entrance.PictureOpacity, 0.01f, 0.99f);
    }

    [Fact]
    public void NameOpacity_BeforeItsDelay_IsZero()
    {
        var entrance = new TitleEntrance();
        entrance.MarkDrawn();

        entrance.Advance(TimeSpan.FromSeconds(0.25));

        Assert.Equal(0, entrance.NameOpacity);
    }

    [Fact]
    public void EntriesOpacity_AfterTheEntrance_IsOne()
    {
        var entrance = new TitleEntrance();
        entrance.MarkDrawn();

        entrance.Advance(TimeSpan.FromSeconds(1.5));

        Assert.Equal(1, entrance.EntriesOpacity);
    }

    [Fact]
    public void Advance_BeforeTheFirstDraw_KeepsThePictureHidden()
    {
        var entrance = new TitleEntrance();

        entrance.Advance(TimeSpan.FromSeconds(1.5));

        Assert.Equal(0, entrance.PictureOpacity);
    }
}
