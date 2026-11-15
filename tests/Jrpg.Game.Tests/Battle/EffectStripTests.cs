using Jrpg.Core.Battle;
using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

// A strip of 128 px frames at its record's pace (decision 654): 0.08 s each while no record lists it, a record's own
// frame time once one does.
public sealed class EffectStripTests
{
    private readonly EffectStrip _strip = EffectStrip.Of(
        new Point(1280, 128), EffectStripDefinition.Of("Burst.png", new Dictionary<string, EffectStripDefinition>()));

    [Fact]
    public void Of_TenFrameWideImage_HasTenFrames()
    {
        Assert.Equal(10, _strip.FrameCount);
    }

    [Fact]
    public void FrameAt_TwoFrameTimes_IsThirdFrame()
    {
        Assert.Equal(2, _strip.FrameAt(TimeSpan.FromSeconds(0.16)));
    }

    [Fact]
    public void FrameAt_PastLastFrame_IsFinished()
    {
        Assert.Null(_strip.FrameAt(TimeSpan.FromSeconds(0.8)));
    }

    [Fact]
    public void FrameAt_ListedPace_UsesItsFrameTime()
    {
        Assert.Equal(2, Fast().FrameAt(TimeSpan.FromSeconds(0.04)));
    }

    [Fact]
    public void FrameAt_ListedPace_EndsAfterItsFrames()
    {
        Assert.Null(Fast().FrameAt(TimeSpan.FromSeconds(0.068)));
    }

    [Fact]
    public void FrameAtImpactTime_BeforeTheLead_IsNone()
    {
        Assert.Null(HitOnFrame(2).FrameAtImpactTime(-TimeSpan.FromSeconds(0.17)));
    }

    [Fact]
    public void FrameAtImpactTime_AtTheLeadsStart_IsTheFirstFrame()
    {
        Assert.Equal(0, HitOnFrame(2).FrameAtImpactTime(-TimeSpan.FromSeconds(0.16)));
    }

    [Fact]
    public void FrameAtImpactTime_AtTheImpact_IsTheHitFrame()
    {
        Assert.Equal(2, HitOnFrame(2).FrameAtImpactTime(TimeSpan.Zero));
    }

    // A lead of ten frames, 0.8 s, is longer than the 0.7 s wind-up: the strip is already on frame 1 as it winds up.
    [Fact]
    public void FrameAtImpactTime_LeadLongerThanTheWindUp_SkipsItsFirstFrames()
    {
        Assert.Equal(1, HitOnFrame(10).FrameAtImpactTime(-TimeSpan.FromSeconds(0.7)));
    }

    [Fact]
    public void FrameAtImpactTime_HitFrameZero_StartsAtTheImpact()
    {
        var strip = HitOnFrame(0);

        Assert.Equal((null, 0), (strip.FrameAtImpactTime(-TimeSpan.FromTicks(1)), strip.FrameAtImpactTime(TimeSpan.Zero)));
    }

    // Twelve 128 px frames of 80 ms, the hit on the given frame.
    private static EffectStrip HitOnFrame(int hitFrame) => EffectStrip.Of(
        new Point(1536, 128), new EffectStripDefinition { File = "Burst.png", FrameMilliseconds = 80, HitFrame = hitFrame });

    // Four 128 px frames of 17 ms.
    private static EffectStrip Fast() =>
        EffectStrip.Of(new Point(512, 128), new EffectStripDefinition { File = "Burst.png", FrameMilliseconds = 17 });
}
