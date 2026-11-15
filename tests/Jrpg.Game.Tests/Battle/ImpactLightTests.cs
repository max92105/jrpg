using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// The impact's light over time (decision 660): the warm flash, the white wash and the floor glow, held 0.1 s and gone
// by 0.2 s; the halo rising from 0.1 s, held from 0.2 s to 0.43 s and gone by 0.5 s.
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
    public void At_PointTwoSeconds_FlashIsDark()
    {
        var look = ImpactLight.At(TimeSpan.FromSeconds(0.2));

        Assert.Equal((0f, 0f, 0f), (look.SceneFlash, look.Wash, look.FloorGlow));
    }

    [Fact]
    public void At_JustBeforePointTwoSeconds_IsStillLit()
    {
        Assert.True(ImpactLight.At(TimeSpan.FromSeconds(0.19)).SceneFlash > 0);
    }

    [Fact]
    public void At_BeforeTheImpact_IsDark()
    {
        Assert.Equal(new ImpactLightLook(0, 0, 0, 0), ImpactLight.At(TimeSpan.FromSeconds(-0.01)));
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

    [Fact]
    public void At_TheImpact_LightsTheFloor()
    {
        Assert.Equal(0.8f, ImpactLight.At(TimeSpan.Zero).FloorGlow);
    }

    [Fact]
    public void At_JustBeforePointTwoSeconds_FloorIsStillLit()
    {
        Assert.True(ImpactLight.At(TimeSpan.FromSeconds(0.19)).FloorGlow > 0);
    }

    [Fact]
    public void At_TheImpact_HasNoHaloYet()
    {
        Assert.Equal(0, ImpactLight.At(TimeSpan.Zero).Halo);
    }

    [Fact]
    public void At_PointOneSeconds_HasNoHaloYet()
    {
        Assert.Equal(0, ImpactLight.At(TimeSpan.FromSeconds(0.1)).Halo);
    }

    [Fact]
    public void At_PointOneFiveSeconds_HaloIsHalfRisen()
    {
        Assert.Equal(0.25f, ImpactLight.At(TimeSpan.FromSeconds(0.15)).Halo, 0.0001f);
    }

    [Fact]
    public void At_PointTwoSeconds_HaloAtItsHeight()
    {
        Assert.Equal(0.5f, ImpactLight.At(TimeSpan.FromSeconds(0.2)).Halo);
    }

    [Fact]
    public void At_PointFourThreeSeconds_HaloStillHeld()
    {
        Assert.Equal(0.5f, ImpactLight.At(TimeSpan.FromSeconds(0.43)).Halo);
    }

    [Fact]
    public void At_PointFourSixFiveSeconds_HaloIsHalfGone()
    {
        Assert.Equal(0.25f, ImpactLight.At(TimeSpan.FromSeconds(0.465)).Halo, 0.0001f);
    }

    [Fact]
    public void At_HalfASecond_HaloIsGone()
    {
        Assert.Equal(0, ImpactLight.At(TimeSpan.FromSeconds(0.5)).Halo);
    }
}
