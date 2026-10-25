using Jrpg.Game.Battle;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

public sealed class ResourceGaugeViewTests
{
    [Fact]
    public void ShowsPips_MaxAtPipsUpTo_IsTrue()
    {
        Assert.True(ResourceGaugeView.ShowsPips(ResourceGaugeView.PipsUpTo));
    }

    [Fact]
    public void ShowsPips_MaxAbove_IsFalse()
    {
        Assert.False(ResourceGaugeView.ShowsPips(ResourceGaugeView.PipsUpTo + 1));
    }

    // The flash lasts 0.25 s, fading evenly.
    [Theory]
    [InlineData(0, 1)]
    [InlineData(0.125, 0.5)]
    [InlineData(0.25, 0)]
    [InlineData(1, 0)]
    public void FlashShare_AfterChange_FadesOverFlashTime(double seconds, float expected)
    {
        Assert.Equal(expected, ResourceGaugeView.FlashShare(TimeSpan.FromSeconds(seconds)), 3);
    }

    [Fact]
    public void LabelColor_DominantForce_IsLit()
    {
        Assert.Equal(UiTheme.TextPrimary, ResourceGaugeView.LabelColor(isDominant: true));
    }

    [Fact]
    public void LabelColor_NotDominant_IsSecondary()
    {
        Assert.Equal(UiTheme.TextSecondary, ResourceGaugeView.LabelColor(isDominant: false));
    }
}
