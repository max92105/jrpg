using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Maps;

public sealed class TileAnimationTests
{
    // Four frames of 140 ms: the waterfall's lower rows.
    private static readonly TileAnimation _waterfall = new() { FrameCount = 4, FrameMilliseconds = 140 };

    [Fact]
    public void FrameAt_Zero_IsTheFirstFrame() => Assert.Equal(0, _waterfall.FrameAt(TimeSpan.Zero));

    [Fact]
    public void FrameAt_JustBeforeTheFrameTime_IsStillTheFirstFrame() =>
        Assert.Equal(0, _waterfall.FrameAt(TimeSpan.FromMilliseconds(140) - TimeSpan.FromTicks(1)));

    [Fact]
    public void FrameAt_AtTheFrameTime_IsTheSecondFrame() => Assert.Equal(1, _waterfall.FrameAt(TimeSpan.FromMilliseconds(140)));

    [Theory]
    [InlineData(560, 0)]
    [InlineData(700, 1)]
    public void FrameAt_PastTheLastFrame_WrapsToTheFirst(int milliseconds, int frame) =>
        Assert.Equal(frame, _waterfall.FrameAt(TimeSpan.FromMilliseconds(milliseconds)));

    // 365 days are 225,257,142 whole frames of 140 ms plus 120 ms: frame 225,257,142 mod 4 = 2.
    [Fact]
    public void FrameAt_AfterAYearOfPlay_StaysInRange() => Assert.Equal(2, _waterfall.FrameAt(TimeSpan.FromDays(365)));

    [Fact]
    public void FrameAt_NegativeTime_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _waterfall.FrameAt(TimeSpan.FromMilliseconds(-1)));
}
