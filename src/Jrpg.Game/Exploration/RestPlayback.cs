using Jrpg.Game.Scenes;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Plays one rest step's beat (decision 257): the fade to black, a moment of black and the fade back in, started on the
/// step's first update; the step's message may show once the screen is back, and reading it finishes the playback so
/// the next rest fades again.
/// </summary>
internal sealed class RestPlayback
{
    // Out, a second of black, back in (placeholder timing).
    private readonly ScreenFade _fade = new(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(0.5));

    /// <summary>
    /// Whether the current rest has started its beat, so its message's own updates don't start it again.
    /// </summary>
    public bool IsStarted { get; private set; }

    /// <summary>Whether the screen is still going black or coming back; the message waits for it.</summary>
    public bool IsFading => _fade.Phase != FadePhase.Idle;

    /// <summary>How black to draw over the map, 0..1.</summary>
    public float Blackness => _fade.Blackness;

    public void Start()
    {
        _fade.Start();
        IsStarted = true;
    }

    public void Advance(TimeSpan delta) => _fade.Update(delta);

    /// <summary>The rest's message was read: the next rest starts its own beat.</summary>
    public void Finish() => IsStarted = false;
}
