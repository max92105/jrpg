namespace Jrpg.Game.Audio;

/// <summary>
/// Which track plays at what gain, which one is leaving and at what gain, and which one waits paused (decisions 416,
/// 417, 496). The track playing fades out first; only once it is gone does the wanted one start, fading in: a loop left
/// for another track then waits paused, dropping any older paused one; the paused one wanted again resumes, the one
/// that fades out before it dropped; silence fades the playing track out and keeps the paused one, for the victory's
/// fanfare. A track that cycles its parts (the title's intro, loop and outro) is never paused: wanted again, it starts
/// over from its first part.
/// </summary>
internal sealed class MusicMix
{
    /// <summary>How long a track takes to fade in, or out (placeholder).</summary>
    public static readonly TimeSpan Fade = TimeSpan.FromSeconds(1);

    private TimeSpan _sinceStart = Fade;
    private TimeSpan _sinceLeft = Fade;
    private float _leftFrom = 1;
    private MusicTrack? _next;

    /// <summary>The track heard now, or null for silence, as while a track fades out.</summary>
    public MusicTrack? Playing { get; private set; }

    /// <summary>The track fading out; null once it is gone, paused or dropped.</summary>
    public MusicTrack? Leaving { get; private set; }

    /// <summary>The track waiting to resume where it paused; null when none does.</summary>
    public MusicTrack? Paused { get; private set; }

    /// <summary>The share of the volume the playing track is at, 0 to 1.</summary>
    public float PlayingGain => Share(_sinceStart);

    /// <summary>The share of the volume the leaving track is at, falling from where it was when it left.</summary>
    public float LeavingGain => _leftFrom * (1 - Share(_sinceLeft));

    /// <summary>The track heard once any fade-out ends: the one playing, or the one waiting for the fade.</summary>
    public MusicTrack? Next => Leaving is null ? Playing : _next;

    public void Want(MusicTrack? track)
    {
        if (track == Next)
        {
            return;
        }

        // A fade-out under way goes on; only what follows it changes.
        _next = track;
        if (Leaving is not null)
        {
            return;
        }

        if (Playing is not { } playing)
        {
            Start(track);
            return;
        }

        Leaving = playing;
        _leftFrom = PlayingGain;
        _sinceLeft = TimeSpan.Zero;
        if (track is not null && track != Paused && !playing.CyclesParts)
        {
            Paused = playing;
        }

        Playing = null;
    }

    public void Advance(TimeSpan delta)
    {
        _sinceStart += delta;
        _sinceLeft += delta;
        if (Leaving is not null && _sinceLeft >= Fade)
        {
            Leaving = null;
            Start(_next);
        }
    }

    // From silence: the paused track resumes, any other starts; both fade in.
    private void Start(MusicTrack? track)
    {
        if (track is not null && track == Paused)
        {
            Paused = null;
        }

        Playing = track;
        _next = null;
        _sinceStart = TimeSpan.Zero;
    }

    private static float Share(TimeSpan elapsed) => (float)Math.Min(1, elapsed / Fade);
}
