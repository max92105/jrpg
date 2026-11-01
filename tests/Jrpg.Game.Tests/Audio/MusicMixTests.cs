using Jrpg.Game.Audio;

namespace Jrpg.Game.Tests.Audio;

// What plays and what waits (decisions 416, 417): a change is a cut, the track left waits paused, and wanting it again
// resumes it with a short fade-in.
public sealed class MusicMixTests
{
    private readonly MusicMix _mix = new();

    [Fact]
    public void Want_NewTrack_PlaysItAtFullGain()
    {
        _mix.Want("forest.wav");

        Assert.Equal("forest.wav", _mix.Playing);
        Assert.Equal(1f, _mix.PlayingGain);
    }

    [Fact]
    public void Want_NewTrack_PausesTheOneBefore()
    {
        _mix.Want("forest.wav");

        _mix.Want("battle.wav");

        Assert.Equal("battle.wav", _mix.Playing);
        Assert.Equal("forest.wav", _mix.Paused);
        Assert.Equal(1f, _mix.PlayingGain);
    }

    [Fact]
    public void Want_ThePausedTrack_ResumesItFromSilence()
    {
        _mix.Want("forest.wav");
        _mix.Want("battle.wav");

        _mix.Want("forest.wav");

        Assert.Equal("forest.wav", _mix.Playing);
        Assert.Equal(0f, _mix.PlayingGain);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Advance_HalfTheFadeIn_GainIsHalf()
    {
        ResumeForest();

        _mix.Advance(MusicMix.ResumeFadeIn / 2);

        Assert.Equal(0.5f, _mix.PlayingGain);
    }

    [Fact]
    public void Advance_PastTheFadeIn_GainIsFull()
    {
        ResumeForest();

        _mix.Advance(MusicMix.ResumeFadeIn * 2);

        Assert.Equal(1f, _mix.PlayingGain);
    }

    // A change during the fade-in is a cut like any other, at full gain.
    [Fact]
    public void Want_NewTrackDuringTheFadeIn_PlaysItAtFullGain()
    {
        ResumeForest();

        _mix.Want("town.wav");

        Assert.Equal(1f, _mix.PlayingGain);
        Assert.Equal("forest.wav", _mix.Paused);
    }

    [Fact]
    public void Want_Silence_DropsThePlayingTrackAndKeepsThePausedOne()
    {
        _mix.Want("forest.wav");
        _mix.Want("battle.wav");

        _mix.Want(null);

        Assert.Null(_mix.Playing);
        Assert.Equal("forest.wav", _mix.Paused);
    }

    // Silence first, as the victory's fanfare: the map's track still comes back where it paused.
    [Fact]
    public void Want_ThePausedTrackAfterSilence_ResumesIt()
    {
        _mix.Want("forest.wav");
        _mix.Want("battle.wav");
        _mix.Want(null);

        _mix.Want("forest.wav");

        Assert.Equal("forest.wav", _mix.Playing);
        Assert.Equal(0f, _mix.PlayingGain);
    }

    [Fact]
    public void Want_NewTrackAfterSilence_KeepsThePausedOne()
    {
        _mix.Want("forest.wav");
        _mix.Want("battle.wav");
        _mix.Want(null);

        _mix.Want("town.wav");

        Assert.Equal("town.wav", _mix.Playing);
        Assert.Equal("forest.wav", _mix.Paused);
    }

    [Fact]
    public void Want_ThirdTrack_DropsTheOlderPausedOne()
    {
        _mix.Want("forest.wav");
        _mix.Want("battle.wav");

        _mix.Want("town.wav");

        Assert.Equal("town.wav", _mix.Playing);
        Assert.Equal("battle.wav", _mix.Paused);
    }

    [Fact]
    public void Want_SameTrack_ChangesNothing()
    {
        ResumeForest();
        _mix.Advance(MusicMix.ResumeFadeIn / 2);

        _mix.Want("forest.wav");

        Assert.Equal("forest.wav", _mix.Playing);
        Assert.Equal(0.5f, _mix.PlayingGain);
    }

    private void ResumeForest()
    {
        _mix.Want("forest.wav");
        _mix.Want("battle.wav");
        _mix.Want("forest.wav");
    }
}
