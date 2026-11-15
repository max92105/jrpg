using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

// The ring's fill by share (decision 626): whole segments lit, the edge one lit by its part, of 48.
public sealed class ProgressRingViewTests
{
    [Fact]
    public void LitSegments_ShareZero_IsNone()
    {
        Assert.Equal(0, ProgressRingView.LitSegments(0));
    }

    [Fact]
    public void LitSegments_ShareOne_IsAll()
    {
        Assert.Equal(ProgressRingView.SegmentCount, ProgressRingView.LitSegments(1));
    }

    [Fact]
    public void LitSegments_ShareHalf_IsTwentyFour()
    {
        Assert.Equal(24, ProgressRingView.LitSegments(0.5f));
    }

    // 0.51 of 48 is 24.48: segment 24 is at the edge, lit 0.48 of full.
    [Fact]
    public void EdgeAlpha_ShareBetweenSegments_IsItsPart()
    {
        Assert.Equal(24, ProgressRingView.LitSegments(0.51f));
        Assert.Equal(0.48f, ProgressRingView.EdgeAlpha(0.51f), precision: 3);
    }

    [Fact]
    public void LitSegments_ShareAboveOne_IsAll()
    {
        Assert.Equal(ProgressRingView.SegmentCount, ProgressRingView.LitSegments(1.5f));
    }
}
