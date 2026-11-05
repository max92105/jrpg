using Jrpg.Game.Audio;

namespace Jrpg.Game.Tests.Audio;

// What plays, what leaves and what waits (decisions 416, 417, 496): a loop fades in while the one before fades out and
// then waits paused; a track with its own intro and outro is neither faded nor paused.
public sealed class MusicMixTests
{
    private static readonly MusicTrack _forest = new("forest/loop.wav");
    private static readonly MusicTrack _battle = new("battle/loop.wav");
    private static readonly MusicTrack _town = new("town/loop.wav");
    private static readonly MusicTrack _title = new("titlescreen/loop.wav", HasIntro: true, HasOutro: true);

    private readonly MusicMix _mix = new();

    [Fact]
    public void Want_NewTrack_PlaysItFromSilence()
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
    public void Want_NewTrack_FadesTheOneBeforeOutAndPausesIt()
    {
        PlayForest();

        _mix.Want(_battle);

        Assert.Equal(_battle, _mix.Playing);
        Assert.Equal(_forest, _mix.Leaving);
        Assert.Equal(1f, _mix.LeavingGain);
        Assert.Equal(_forest, _mix.Paused);
    }

    [Fact]
    public void Advance_HalfTheFade_LeavingGainIsHalf()
    {
        PlayForest();
        _mix.Want(_battle);

        _mix.Advance(MusicMix.Fade / 2);

        Assert.Equal(0.5f, _mix.LeavingGain);
    }

    [Fact]
    public void Advance_PastTheFade_TheTrackLeavingIsGoneAndStillPaused()
    {
        PlayForest();
        _mix.Want(_battle);

        _mix.Advance(MusicMix.Fade);

        Assert.Null(_mix.Leaving);
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

    // The older track's fade-out is cut: at most two instances, the one playing and the one leaving or paused.
    [Fact]
    public void Want_NewTrackDuringTheFadeOut_CutsTheTrackLeaving()
    {
        PlayForest();
        _mix.Want(_battle);
        _mix.Advance(MusicMix.Fade / 2);

        _mix.Want(_town);

        Assert.Equal(_battle, _mix.Leaving);
        Assert.Equal(_battle, _mix.Paused);
    }

    [Fact]
    public void Want_ThePausedTrack_ResumesItFromSilenceAndFadesThePlayingOneOut()
    {
        PlayForest();
        _mix.Want(_battle);
        _mix.Advance(MusicMix.Fade);

        _mix.Want(_forest);

        Assert.Equal(_forest, _mix.Playing);
        Assert.Equal(0f, _mix.PlayingGain);
        Assert.Equal(_battle, _mix.Leaving);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Want_TheTrackStillFadingOut_RisesAgainFromWhereItIs()
    {
        PlayForest();
        _mix.Want(_battle);
        _mix.Advance(MusicMix.Fade / 2);

        _mix.Want(_forest);

        Assert.Equal(_forest, _mix.Playing);
        Assert.Equal(0.5f, _mix.PlayingGain);
        Assert.Equal(_battle, _mix.Leaving);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Want_Silence_FadesThePlayingTrackOutAndKeepsThePausedOne()
    {
        PlayForest();
        _mix.Want(_battle);

        _mix.Want(null);

        Assert.Null(_mix.Playing);
        Assert.Equal(_battle, _mix.Leaving);
        Assert.Equal(_forest, _mix.Paused);
    }

    // Silence first, as the victory's fanfare: the map's track still comes back where it paused.
    [Fact]
    public void Want_ThePausedTrackAfterSilence_ResumesIt()
    {
        PlayForest();
        _mix.Want(_battle);
        _mix.Want(null);

        _mix.Want(_forest);

        Assert.Equal(_forest, _mix.Playing);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Want_NewTrackAfterSilence_KeepsThePausedOne()
    {
        PlayForest();
        _mix.Want(_battle);
        _mix.Want(null);

        _mix.Want(_town);

        Assert.Equal(_town, _mix.Playing);
        Assert.Equal(_forest, _mix.Paused);
    }

    [Fact]
    public void Want_ThirdTrack_DropsTheOlderPausedOne()
    {
        PlayForest();
        _mix.Want(_battle);

        _mix.Want(_town);

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

    // Its intro fades in by itself.
    [Fact]
    public void Want_TrackWithAnIntro_PlaysItAtFullGain()
    {
        _mix.Want(_title);

        Assert.Equal(1f, _mix.PlayingGain);
    }

    // Its outro ends it, so it neither fades out nor waits to resume.
    [Fact]
    public void Want_NewTrackOverOneWithAnOutro_LeavesItAtOnceUnpaused()
    {
        _mix.Want(_title);

        _mix.Want(_town);

        Assert.Equal(_town, _mix.Playing);
        Assert.Null(_mix.Leaving);
        Assert.Null(_mix.Paused);
    }

    [Fact]
    public void Want_NewTrackOverOneWithAnOutro_KeepsTheOlderPausedOne()
    {
        PlayForest();
        _mix.Want(_title);

        _mix.Want(_town);

        Assert.Equal(_forest, _mix.Paused);
    }

    private void PlayForest()
    {
        _mix.Want(_forest);
        _mix.Advance(MusicMix.Fade);
    }
}
