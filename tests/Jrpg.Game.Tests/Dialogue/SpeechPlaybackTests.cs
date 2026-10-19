using Jrpg.Game.Dialogue;

namespace Jrpg.Game.Tests.Dialogue;

public sealed class SpeechPlaybackTests
{
    private readonly SpeechPlayback _playback = new(["Hello.", "Goodbye."]);

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
}
