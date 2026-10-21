using Jrpg.Game.Battle;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Battle;

// A strip of 128 px frames, 0.08 s each.
public sealed class EffectStripTests
{
    private readonly EffectStrip _strip = EffectStrip.Of(new Point(1280, 128));

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
}
