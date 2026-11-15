using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// One frame time for a loop (decision 632): the most common delay, the first of equals, under 20 ms read as 100 ms.
public sealed class GifFrameTimingTests
{
    [Fact]
    public void FrameMilliseconds_AllEqual_IsThatDelay()
    {
        Assert.Equal(80, GifFrameTiming.FrameMilliseconds(Delayed(8, 8, 8)));
    }

    [Fact]
    public void FrameMilliseconds_Mixed_IsTheMostCommon()
    {
        Assert.Equal(120, GifFrameTiming.FrameMilliseconds(Delayed(8, 12, 12, 30)));
    }

    [Fact]
    public void FrameMilliseconds_TieInCount_IsTheFirstInFrameOrder()
    {
        Assert.Equal(300, GifFrameTiming.FrameMilliseconds(Delayed(30, 8, 8, 30)));
    }

    [Fact]
    public void FrameMilliseconds_NoTimingBlock_IsHundred()
    {
        Assert.Equal(100, GifFrameTiming.FrameMilliseconds(Delayed(0, 0)));
    }

    [Fact]
    public void FrameMilliseconds_TenMilliseconds_IsHundred()
    {
        Assert.Equal(100, GifFrameTiming.FrameMilliseconds(Delayed(1)));
    }

    [Fact]
    public void FrameMilliseconds_TwentyMilliseconds_IsTwenty()
    {
        Assert.Equal(20, GifFrameTiming.FrameMilliseconds(Delayed(2)));
    }

    [Fact]
    public void AreDelaysEqual_Mixed_IsFalse()
    {
        Assert.False(GifFrameTiming.AreDelaysEqual(Delayed(8, 12)));
    }

    // 0 and 10 ms both play as 100 ms, so a GIF mixing them plays evenly.
    [Fact]
    public void AreDelaysEqual_ShortDelaysPlayingAlike_IsTrue()
    {
        Assert.True(GifFrameTiming.AreDelaysEqual(Delayed(0, 1, 10)));
    }

    private static GifFrame[] Delayed(params int[] centiseconds) =>
        [.. centiseconds.Select(delay => new GifFrame(0, 0, 1, 1, [0xFF000000], delay, GifDisposal.Keep))];
}
