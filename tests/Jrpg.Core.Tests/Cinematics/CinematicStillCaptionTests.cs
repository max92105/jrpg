using Jrpg.Core.Cinematics;

namespace Jrpg.Core.Tests.Cinematics;

// A still's subtitles show one after the other from its start, each for its own seconds, so they follow a voice-over;
// an empty line is a pause.
public sealed class CinematicStillCaptionTests
{
    private static readonly StillCaption _first = new() { Text = "He left.", Seconds = 2 };
    private static readonly StillCaption _pause = new() { Text = "", Seconds = 0.5 };
    private static readonly StillCaption _last = new() { Text = "At dawn.", Seconds = 1 };

    private static readonly CinematicStill _still = new()
    {
        ImageFile = "intro/1.png",
        Seconds = 5,
        Captions = [_first, _pause, _last],
    };

    [Theory]
    [InlineData(0)]
    [InlineData(1.99)]
    public void CaptionAt_WithinTheFirstLinesTime_IsTheFirst(double seconds)
    {
        Assert.Same(_first, _still.CaptionAt(TimeSpan.FromSeconds(seconds)));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(2.49)]
    public void CaptionAt_DuringAPause_IsNull(double seconds)
    {
        Assert.Null(_still.CaptionAt(TimeSpan.FromSeconds(seconds)));
    }

    [Theory]
    [InlineData(2.5)]
    [InlineData(3.49)]
    public void CaptionAt_WithinTheLastLinesTime_IsTheLast(double seconds)
    {
        Assert.Same(_last, _still.CaptionAt(TimeSpan.FromSeconds(seconds)));
    }

    [Fact]
    public void CaptionAt_OnceTheLastLinesTimeIsOver_IsNull()
    {
        Assert.Null(_still.CaptionAt(TimeSpan.FromSeconds(3.5)));
    }

    [Fact]
    public void CaptionAt_StillWithoutSubtitles_IsNull()
    {
        Assert.Null((_still with { Captions = [] }).CaptionAt(TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void Duration_OfACaption_IsItsSeconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(0.5), _pause.Duration);
    }

    // The lists are two instances: the generated Equals would compare them by reference.
    [Fact]
    public void Equals_SameSubtitlesInAnotherList_IsTrue()
    {
        var copy = _still with { Captions = [_first with { }, _pause with { }, _last with { }] };

        Assert.Equal(_still, copy);
        Assert.Equal(_still.GetHashCode(), copy.GetHashCode());
    }

    [Fact]
    public void Equals_ALinesSecondsChanged_IsFalse()
    {
        Assert.NotEqual(_still, _still with { Captions = [_first, _pause, _last with { Seconds = 1.5 }] });
    }

    [Fact]
    public void Equals_LinesInAnotherOrder_IsFalse()
    {
        Assert.NotEqual(_still, _still with { Captions = [_last, _pause, _first] });
    }

    [Fact]
    public void Equals_AnotherPicture_IsFalse()
    {
        Assert.NotEqual(_still, _still with { ImageFile = "intro/2.png" });
    }
}
