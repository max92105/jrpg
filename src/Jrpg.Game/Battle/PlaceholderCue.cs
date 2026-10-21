namespace Jrpg.Game.Battle;

/// <summary>
/// The stand-in for an effect with no animation yet (decision 101): the action's name over the target, fading in over
/// 0.15 s, shown 0.3 s, fading out over 0.15 s, so its trigger and timing can be checked. Question 106 is still open.
/// </summary>
internal static class PlaceholderCue
{
    private static readonly TimeSpan _fadeIn = TimeSpan.FromSeconds(0.15);
    private static readonly TimeSpan _shown = TimeSpan.FromSeconds(0.3);
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.15);

    /// <param name="sinceShown">Time since the effect landed.</param>
    /// <returns>0 hidden to 1 fully shown.</returns>
    public static float OpacityAt(TimeSpan sinceShown) =>
        sinceShown < _fadeIn ? (float)(sinceShown / _fadeIn)
        : sinceShown < _fadeIn + _shown ? 1
        : Math.Max(0, 1 - (float)((sinceShown - _fadeIn - _shown) / _fadeOut));
}
