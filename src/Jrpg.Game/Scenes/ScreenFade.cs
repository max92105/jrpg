namespace Jrpg.Game.Scenes;

/// <summary>
/// A fade to black and back: out, a short hold on black (when whatever the fade hides is changed), then in. Pure
/// timing; the scene draws the black.
/// </summary>
internal sealed class ScreenFade(TimeSpan fadeOut, TimeSpan hold, TimeSpan fadeIn)
{
    private TimeSpan _timeInPhase;

    public FadePhase Phase { get; private set; }

    /// <summary>0 clear to 1 black.</summary>
    public float Blackness => Phase switch
    {
        FadePhase.FadingOut => (float)(_timeInPhase / fadeOut),
        FadePhase.Holding => 1,
        FadePhase.FadingIn => 1 - (float)(_timeInPhase / fadeIn),
        _ => 0,
    };

    /// <exception cref="InvalidOperationException">A fade is already playing.</exception>
    public void Start()
    {
        if (Phase != FadePhase.Idle)
        {
            throw new InvalidOperationException($"A fade is already playing ({Phase}).");
        }

        Phase = FadePhase.FadingOut;
        _timeInPhase = TimeSpan.Zero;
    }

    /// <summary>Plays on; time left over at the end of a phase carries into the next, so the total length is exact.</summary>
    public void Update(TimeSpan delta)
    {
        if (Phase == FadePhase.Idle)
        {
            return;
        }

        _timeInPhase += delta;
        while (Phase != FadePhase.Idle && _timeInPhase >= DurationOf(Phase))
        {
            _timeInPhase -= DurationOf(Phase);
            Phase = NextAfter(Phase);
        }
    }

    private static FadePhase NextAfter(FadePhase phase) => phase switch
    {
        FadePhase.FadingOut => FadePhase.Holding,
        FadePhase.Holding => FadePhase.FadingIn,
        _ => FadePhase.Idle,
    };

    private TimeSpan DurationOf(FadePhase phase) => phase switch
    {
        FadePhase.FadingOut => fadeOut,
        FadePhase.Holding => hold,
        FadePhase.FadingIn => fadeIn,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null),
    };
}
