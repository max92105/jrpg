namespace Jrpg.Game.Battle;

/// <summary>How the defeat screen comes in: the title fades in slowly, then the choices.</summary>
internal sealed class DefeatEntrance
{
    // Placeholder timing: the title's fade, the choices' delay and their 0.3 s fade.
    private static readonly TimeSpan _titleFade = TimeSpan.FromSeconds(0.6);
    private static readonly TimeSpan _choicesDelay = TimeSpan.FromSeconds(0.7);
    private static readonly TimeSpan _choicesFade = TimeSpan.FromSeconds(0.3);

    private TimeSpan _elapsed;

    /// <summary>Whether the choices are fully shown, so they take input.</summary>
    public bool IsComplete => _elapsed >= _choicesDelay + _choicesFade;

    public float TitleOpacity => ShareOf(_elapsed, _titleFade);

    public float ChoicesOpacity => ShareOf(_elapsed - _choicesDelay, _choicesFade);

    public void Update(TimeSpan delta) => _elapsed += delta;

    private static float ShareOf(TimeSpan time, TimeSpan length) => Math.Clamp((float)(time / length), 0, 1);
}
