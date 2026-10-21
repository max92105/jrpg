using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// A cue: 0.15 s in, 0.3 s shown, 0.15 s out.
public sealed class PlaceholderCueTests
{
    [Fact]
    public void OpacityAt_WhileShown_IsFullyVisible()
    {
        Assert.Equal(1, PlaceholderCue.OpacityAt(TimeSpan.FromSeconds(0.3)));
    }

    [Fact]
    public void OpacityAt_PastLength_IsHidden()
    {
        Assert.Equal(0, PlaceholderCue.OpacityAt(TimeSpan.FromSeconds(0.7)));
    }

    [Theory]
    [InlineData(0.075)]
    [InlineData(0.525)]
    public void OpacityAt_HalfwayThroughAFade_IsHalfVisible(double seconds)
    {
        Assert.Equal(0.5f, PlaceholderCue.OpacityAt(TimeSpan.FromSeconds(seconds)), 3);
    }
}
