using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// The impact's light over time (decision 660): the warm flash and the white wash, held 0.1 s and gone by 0.2 s.
public sealed class ImpactLightTests
{
    [Fact]
    public void At_TheImpact_FlashesAtItsHeight()
    {
        Assert.Equal(0.45f, ImpactLight.At(TimeSpan.Zero).SceneFlash);
    }

    [Fact]
    public void At_PointOneSeconds_StillNearFull()
    {
        Assert.Equal(0.45f, ImpactLight.At(TimeSpan.FromSeconds(0.1)).SceneFlash);
    }

    [Fact]
    public void At_PointOneFiveSeconds_IsHalfway()
    {
        Assert.Equal(0.225f, ImpactLight.At(TimeSpan.FromSeconds(0.15)).SceneFlash, 0.0001f);
    }

    [Fact]
    public void At_PointTwoSeconds_IsDark()
    {
        Assert.Equal(new ImpactLightLook(0, 0), ImpactLight.At(TimeSpan.FromSeconds(0.2)));
    }

    [Fact]
    public void At_JustBeforePointTwoSeconds_IsStillLit()
    {
        Assert.True(ImpactLight.At(TimeSpan.FromSeconds(0.19)).SceneFlash > 0);
    }

    [Fact]
    public void At_BeforeTheImpact_IsDark()
    {
        Assert.Equal(new ImpactLightLook(0, 0), ImpactLight.At(TimeSpan.FromSeconds(-0.01)));
    }

    [Fact]
    public void At_TheImpact_WashesAtItsHeight()
    {
        Assert.Equal(0.6f, ImpactLight.At(TimeSpan.Zero).Wash);
    }

    [Fact]
    public void WashAt_TheTarget_IsTheWholeWash()
    {
        Assert.Equal(0.6f, ImpactLight.WashAt(0, 0.6f));
    }

    [Fact]
    public void WashAt_HalfTheReach_IsHalf()
    {
        Assert.Equal(0.3f, ImpactLight.WashAt(200, 0.6f), 0.0001f);
    }

    [Fact]
    public void WashAt_TheReach_IsNone()
    {
        Assert.Equal(0, ImpactLight.WashAt(400, 0.6f));
    }

    [Fact]
    public void WashAt_PastTheReach_IsNone()
    {
        Assert.Equal(0, ImpactLight.WashAt(600, 0.6f));
    }
}
