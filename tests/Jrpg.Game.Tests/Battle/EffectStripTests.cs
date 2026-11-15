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

    // Four 128 px frames of 17 ms.
    private static EffectStrip Fast() =>
        EffectStrip.Of(new Point(512, 128), new EffectStripDefinition { File = "Burst.png", FrameMilliseconds = 17 });
}
