using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class StatBarTests
{
    private readonly StatBar _bar = new(new UiTextures(), UiTheme.HpBar);

    [Fact]
    public void Show_HalfOfMax_FillsHalf()
    {
        _bar.Show(30, 60);

        Assert.Equal(0.5f, _bar.Fraction);
    }

    [Fact]
    public void Show_ZeroMax_IsEmpty()
    {
        _bar.Show(0, 0);

        Assert.Equal(0, _bar.Fraction);
    }

    [Fact]
    public void Show_OverMax_StaysFull()
    {
        _bar.Show(90, 60);

        Assert.Equal(1, _bar.Fraction);
    }
}
