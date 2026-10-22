using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

public sealed class EasingTests
{
    [Fact]
    public void ShareOf_PastTheLength_IsOne()
    {
        var share = Easing.ShareOf(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(2));

        Assert.Equal(1f, share);
    }

    [Fact]
    public void ShareOf_Halfway_IsHalf()
    {
        var share = Easing.ShareOf(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));

        Assert.Equal(0.5f, share);
    }

    [Fact]
    public void CubicOut_Half_IsSevenEighths()
    {
        var eased = Easing.CubicOut(0.5f);

        Assert.Equal(0.875f, eased, 6);
    }
}
