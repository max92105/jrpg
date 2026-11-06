using Jrpg.Game.Audio;

namespace Jrpg.Game.Tests.Audio;

// What plays, what leaves and what waits (decisions 416, 417, 496): the track playing fades out first, then the wanted
// one fades in; a loop left for another waits paused; a track that cycles its parts is never paused.
public sealed class MusicMixTests
{
    private static readonly MusicTrack _forest = new("forest/loop.wav");
    private static readonly MusicTrack _battle = new("battle/loop.wav");
    private static readonly MusicTrack _town = new("town/loop.wav");
    private static readonly MusicTrack _title = new("titlescreen/loop.wav", HasIntro: true, HasOutro: true);

    private readonly MusicMix _mix = new();

    [Fact]
    public void Want_NewTrackFromSilence_PlaysItFromZero()
    {
        _mix.Want(_forest);

        Assert.Equal(_forest, _mix.Playing);
        Assert.Equal(0f, _mix.PlayingGain);
        Assert.Null(_mix.Leaving);
    }

    [Fact]
    public void Advance_HalfTheFade_PlayingGainIsHalf()
    {
        _mix.Want(_forest);

        _mix.Advance(MusicMix.Fade / 2);

        Assert.Equal(0.5f, _mix.PlayingGain);
    }

    [Fact]
    public void Advance_PastTheFade_PlayingGainIsFull()
    {
        _mix.Want(_forest);

        _mix.Advance(MusicMix.Fade * 2);

        Assert.Equal(1f, _mix.PlayingGain);
    }

    [Fact]
    public void Want_NewTrack_FadesTheOneBeforeOutFirstAndPausesIt()
    {
        Play(_forest);

        _mix.Want(_battle);

        Assert.Null(_mix.Playing);
        Assert.Equal(_forest, _mix.Leaving);
        Assert.Equal(1f, _mix.LeavingGain);
        Assert.Equal(_forest, _mix.Paused);
    }

    [Fact]
    public void Advance_HalfTheFade_LeavingGainIsHalf()
    {
        Play(_forest);
        _mix.Want(_battle);

        _mix.Advance(MusicMix.Fade / 2);

        Assert.Equal(0.5f, _mix.LeavingGain);
        Assert.Null(_mix.Playing);
    }

    [Fact]
    public void Advance_PastTheFade_StartsTheWantedTrackFromZero()
    {
        Play(_forest);
        _mix.Want(_battle);

        _mix.Advance(MusicMix.Fade);

        Assert.Null(_mix.Leaving);
        Assert.Equal(_battle, _mix.Playing);
        Assert.Equal(0f, _mix.PlayingGain);
        Assert.Equal(_forest, _mix.Paused);
    }

    [Fact]
    public void Want_NewTrackDuringTheFadeIn_FadesTheOldOneOutFromWhereItWas()
    {
        _mix.Want(_forest);
        _mix.Advance(MusicMix.Fade / 2);

        _mix.Want(_battle);
        _mix.Advance(MusicMix.Fade / 2);

        Assert.Equal(0.25f, _mix.LeavingGain);
    }

    // The fade-out under way goes on; only the track that follows it changes.
    [Fact]
    public void Want_AnotherTrackDuringTheFadeOut_KeepsTheFadeAndChangesWhatFollows()
    {
        Play(_forest);
        _mix.Want(_battle);
        _mix.Advance(MusicMix.Fade / 2);

        _mix.Want(_town);

        Assert.Equal(_forest, _mix.Leaving);
        Assert.Equal(0.5f, _mix.LeavingGain);
        _mix.Advance(MusicMix.Fade / 2);
        Assert.Equal(_town, _mix.Playing);
        Assert.Equal(_forest, _mix.Paused);
    }

    [Fact]
    public void Want_TheTrackFadingOut_ResumesItOnceItsFadeOutEnds()
    {
        Play(_forest);
        _mix.Want(_battle);
        _mix.Advance(MusicMix.Fade / 2);

        _mix.Want(_forest);
        _mix.Advance(MusicMix.Fade / 2);

        Assert.Equal(_forest, _mix.Playing);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Want_ThePausedTrack_FadesThePlayingOneOutThenResumesItFromZero()
    {
        Play(_forest);
        Switch(_battle);

        _mix.Want(_forest);

        Assert.Equal(_battle, _mix.Leaving);
        Assert.Null(_mix.Playing);
        _mix.Advance(MusicMix.Fade);
        Assert.Equal(_forest, _mix.Playing);
        Assert.Equal(0f, _mix.PlayingGain);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Want_Silence_FadesThePlayingTrackOutAndKeepsThePausedOne()
    {
        Play(_forest);
        Switch(_battle);

        _mix.Want(null);
        _mix.Advance(MusicMix.Fade);

        Assert.Null(_mix.Playing);
        Assert.Null(_mix.Leaving);
        Assert.Equal(_forest, _mix.Paused);
    }

    // Silence first, as the victory's fanfare: the map's track still comes back where it paused.
    [Fact]
    public void Want_ThePausedTrackAfterSilence_ResumesItAtOnce()
    {
        Play(_forest);
        Switch(_battle);
        Switch(null);

        _mix.Want(_forest);

        Assert.Equal(_forest, _mix.Playing);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Want_NewTrackAfterSilence_KeepsThePausedOne()
    {
        Play(_forest);
        Switch(_battle);
        Switch(null);

        _mix.Want(_town);

        Assert.Equal(_town, _mix.Playing);
        Assert.Equal(_forest, _mix.Paused);
    }

    [Fact]
    public void Want_ThirdTrack_DropsTheOlderPausedOne()
    {
        Play(_forest);
        Switch(_battle);

        Switch(_town);

        Assert.Equal(_town, _mix.Playing);
        Assert.Equal(_battle, _mix.Paused);
    }

    [Fact]
    public void Want_SameTrack_ChangesNothing()
    {
        _mix.Want(_forest);
        _mix.Advance(MusicMix.Fade / 2);

        _mix.Want(_forest);

        Assert.Equal(_forest, _mix.Playing);
        Assert.Equal(0.5f, _mix.PlayingGain);
    }

    [Fact]
    public void Want_TheTrackAlreadyWantedNext_ChangesNothing()
    {
        Play(_forest);
        _mix.Want(_battle);
        _mix.Advance(MusicMix.Fade / 2);

        _mix.Want(_battle);

        Assert.Equal(0.5f, _mix.LeavingGain);
        Assert.Equal(_forest, _mix.Leaving);
    }

    // The title starts over from its intro, so it isn't kept.
    [Fact]
    public void Want_NewTrackOverOneThatCyclesParts_NeverPausesIt()
    {
        Play(_title);

        Switch(_town);

        Assert.Equal(_town, _mix.Playing);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Want_NewTrackOverOneThatCyclesParts_KeepsTheOlderPausedOne()
    {
        Play(_forest);
        Switch(_title);

        Switch(_town);

        Assert.Equal(_forest, _mix.Paused);
    }

    private void Play(MusicTrack track)
    {
        _mix.Want(track);
        _mix.Advance(MusicMix.Fade);
    }

    // Through the fade-out and the fade-in, so the track is heard at full gain.
    private void Switch(MusicTrack? track)
    {
        _mix.Want(track);
        _mix.Advance(MusicMix.Fade);
        _mix.Advance(MusicMix.Fade);
    }
}
