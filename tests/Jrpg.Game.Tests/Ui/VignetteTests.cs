using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class VignetteTests
{
    private const int Width = 160;
    private const int Height = 90;

    [Fact]
    public void Colors_Center_IsClear()
    {
        var colors = Vignette.Colors(UiTheme.BattleVignette, Width, Height);

        Assert.Equal(0, colors[((Height / 2) * Width) + (Width / 2)].A);
    }

    [Fact]
    public void Colors_Corner_IsDarkest()
    {
        var colors = Vignette.Colors(UiTheme.BattleVignette, Width, Height);

        Assert.Equal(colors.Max(color => color.A), colors[0].A);
        Assert.True(colors[0].A > colors[Width / 2].A, "The corner should be darker than the middle of the top edge.");
    }
}
