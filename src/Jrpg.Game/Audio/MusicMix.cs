namespace Jrpg.Game.Audio;

/// <summary>
/// Which track plays at what gain, and which one waits paused (decisions 416, 417). A new track is a cut at full gain
/// and pauses the one before, dropping any older paused one; the paused one wanted again resumes, its gain rising over
/// <see cref="ResumeFadeIn"/>, and the one playing is dropped; silence drops the one playing and keeps the paused one,
/// for the victory's fanfare.
/// </summary>
internal sealed class MusicMix
{
    /// <summary>How long a resumed track takes to reach full gain (placeholder).</summary>
    public static readonly TimeSpan ResumeFadeIn = TimeSpan.FromSeconds(0.5);

    private TimeSpan _sinceResume = ResumeFadeIn;

    /// <summary>The track heard now, or null for silence.</summary>
    public string? Playing { get; private set; }

    /// <summary>The track waiting to resume where it paused; null when none does.</summary>
    public string? Paused { get; private set; }

    /// <summary>The share of the volume the playing track is at, 0 to 1.</summary>
    public float PlayingGain => (float)Math.Min(1, _sinceResume / ResumeFadeIn);

    public void Want(string? file)
    {
        if (file == Playing)
        {
            return;
        }

        if (file is not null && file == Paused)
        {
            Paused = null;
            _sinceResume = TimeSpan.Zero;
        }
        else
        {
            // Silence leaves nothing playing to pause, so the track already waiting keeps its place.
            Paused = file is null ? Paused : Playing ?? Paused;
            _sinceResume = ResumeFadeIn;
        }

        Playing = file;
    }

    public void Advance(TimeSpan delta) => _sinceResume += delta;
}
