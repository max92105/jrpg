using Jrpg.Game.Dialogue;

namespace Jrpg.Game.Tests.Dialogue;

public sealed class SpeechPlaybackTests
{
    private static readonly TimeSpan _characterTime = TimeSpan.FromMilliseconds(30);

    // Shown whole, so Confirm always turns the page.
    private readonly SpeechPlayback _playback = new(["Hello.", "Goodbye."], TimeSpan.Zero);

    [Fact]
    public void CurrentPage_New_IsFirstPage()
    {
        Assert.Equal("Hello.", _playback.CurrentPage);
        Assert.False(_playback.IsFinished);
    }

    [Fact]
    public void Confirm_NotLastPage_ShowsNextPage()
    {
        _playback.Confirm();

        Assert.Equal("Goodbye.", _playback.CurrentPage);
        Assert.Equal(1, _playback.PageIndex);
        Assert.False(_playback.IsFinished);
    }

    [Fact]
    public void Confirm_OnLastPage_Finishes()
    {
        _playback.Confirm();

        _playback.Confirm();

        Assert.True(_playback.IsFinished);
    }

    [Fact]
    public void Confirm_AfterFinished_Throws()
    {
        _playback.Confirm();
        _playback.Confirm();

        Assert.Throws<InvalidOperationException>(_playback.Confirm);
    }

    [Fact]
    public void Confirm_WhileTyping_CompletesPageWithoutAdvancing()
    {
        var typing = new SpeechPlayback(["Hello.", "Goodbye."], _characterTime);
        typing.Update(_characterTime * 2);

        typing.Confirm();

        Assert.Equal(0, typing.PageIndex);
        Assert.Equal("Hello.".Length, typing.VisibleLength);
    }

    [Fact]
    public void Confirm_TurningPage_StartsTypingNextPage()
    {
        var typing = new SpeechPlayback(["Hello.", "Goodbye."], _characterTime);
        typing.Update(TimeSpan.FromSeconds(1));

        typing.Confirm();

        Assert.Equal(1, typing.PageIndex);
        Assert.Equal(0, typing.VisibleLength);
    }

    [Fact]
    public void IsMarkerVisible_WhileTyping_IsFalse()
    {
        var typing = new SpeechPlayback(["Hello."], _characterTime);

        typing.Update(_characterTime);

        Assert.False(typing.IsMarkerVisible);
    }

    [Fact]
    public void IsMarkerVisible_CompletePage_BlinksEveryHalfSecond()
    {
        var atFirst = _playback.IsMarkerVisible;
        _playback.Update(TimeSpan.FromSeconds(0.5));
        var afterHalfSecond = _playback.IsMarkerVisible;
        _playback.Update(TimeSpan.FromSeconds(0.5));

        Assert.True(atFirst);
        Assert.False(afterHalfSecond);
        Assert.True(_playback.IsMarkerVisible);
    }
}
