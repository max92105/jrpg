using Jrpg.Core.Events;
using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

public sealed class StillPlaybackTests
{
    [Fact]
    public void Opacity_AtStart_IsZero()
    {
        var playback = new StillPlayback(new StillStep("Placeholder1.png", 4, Caption: null));

        Assert.Equal(0f, playback.Opacity);
    }

    [Fact]
    public void Opacity_AfterShotFade_IsOne()
    {
        var playback = new StillPlayback(new StillStep("Placeholder1.png", 4, Caption: null));

        playback.Update(StillPlayback.ShotFade);

        Assert.Equal(1f, playback.Opacity);
    }

    [Fact]
    public void Opacity_HalfwayThroughTheFadeOut_IsHalf()
    {
        var playback = new StillPlayback(new StillStep("Placeholder1.png", 4, Caption: null));

        playback.Update(TimeSpan.FromSeconds(4) - (StillPlayback.ShotFade / 2));

        Assert.Equal(0.5f, playback.Opacity, precision: 3);
    }

    [Fact]
    public void Opacity_StillShorterThanTwoFades_PeaksAtHalfway()
    {
        var playback = new StillPlayback(new StillStep("Placeholder1.png", 1, Caption: null));

        playback.Update(TimeSpan.FromSeconds(0.5));

        Assert.Equal(1f, playback.Opacity, precision: 3);
    }

    [Fact]
    public void IsOver_JustBeforeItsSeconds_IsFalse()
    {
        var playback = new StillPlayback(new StillStep("Placeholder1.png", 4, Caption: null));

        playback.Update(TimeSpan.FromSeconds(3.99));

        Assert.False(playback.IsOver);
    }

    [Fact]
    public void IsOver_ExactlyItsSeconds_IsTrue()
    {
        var playback = new StillPlayback(new StillStep("Placeholder1.png", 4, Caption: null));

        playback.Update(TimeSpan.FromSeconds(4));

        Assert.True(playback.IsOver);
    }
}
