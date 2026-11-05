namespace Jrpg.Game.Audio;

/// <summary>
/// Which track plays at what gain, which one is leaving and at what gain, and which one waits paused (decisions 416,
/// 417, 496). A new track fades in while the one before fades out and then waits paused, dropping any older paused one;
/// the paused one wanted again resumes and fades in while the one playing fades out and is dropped; silence fades the
/// one playing out and keeps the paused one, for the victory's fanfare. A track with its own intro and outro is never
/// faded nor paused: it leaves the mix at once, for its outro, and starts over from its intro.
/// </summary>
internal sealed class MusicMix
{
    /// <summary>How long a loop takes to fade in, or out (placeholder).</summary>
    public static readonly TimeSpan Fade = TimeSpan.FromSeconds(1);

    private TimeSpan _sinceStart = Fade;
    private TimeSpan _sinceLeft = Fade;
    private float _leftFrom = 1;

    /// <summary>The track heard now, or null for silence.</summary>
    public MusicTrack? Playing { get; private set; }

    /// <summary>The track fading out; null once it is gone, paused or dropped.</summary>
    public MusicTrack? Leaving { get; private set; }

    /// <summary>The track waiting to resume where it paused; null when none does.</summary>
    public MusicTrack? Paused { get; private set; }

    /// <summary>The share of the volume the playing track is at, 0 to 1.</summary>
    public float PlayingGain => Playing is { HasIntro: true } ? 1 : Share(_sinceStart);

    /// <summary>The share of the volume the leaving track is at, falling from where it was when it left.</summary>
    public float LeavingGain => _leftFrom * (1 - Share(_sinceLeft));

    public void Want(MusicTrack? track)
    {
        if (track == Playing)
        {
            return;
        }

        // Wanted back while still fading out, a track rises again from where it is, not from silence.
        var startFrom = track is not null && track == Leaving ? LeavingGain : 0;
        Leave();
        if (track is not null && track == Paused)
        {
            Paused = null;
        }
        else if (track is not null && Playing is { HasOutro: false })
        {
            Paused = Playing;
        }

        Playing = track;
        _sinceStart = Fade * startFrom;
    }

    public void Advance(TimeSpan delta)
    {
        _sinceStart += delta;
        _sinceLeft += delta;
        if (_sinceLeft >= Fade)
        {
            Leaving = null;
        }
    }

    // A second change before the fade-out ends cuts the track leaving; its outro ends a track with one.
    private void Leave()
    {
        Leaving = Playing is { HasOutro: false } ? Playing : null;
        _leftFrom = PlayingGain;
        _sinceLeft = TimeSpan.Zero;
    }

    private static float Share(TimeSpan elapsed) => (float)Math.Min(1, elapsed / Fade);
}
