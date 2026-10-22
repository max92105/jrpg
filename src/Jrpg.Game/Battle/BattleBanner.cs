namespace Jrpg.Game.Battle;

/// <summary>
/// The ribbon at the top of the battle: a short message fades in over 0.15 s, holds 1.2 s, then fades out over 0.3 s.
/// Pure timing; <see cref="BattleBannerView"/> draws it.
/// </summary>
internal sealed class BattleBanner
{
    private static readonly TimeSpan _fadeIn = TimeSpan.FromSeconds(0.15);
    private static readonly TimeSpan _hold = TimeSpan.FromSeconds(1.2);
    private static readonly TimeSpan _fadeOut = TimeSpan.FromSeconds(0.3);

    // Starts past the end, so nothing shows until the first message.
    private TimeSpan _elapsed = _fadeIn + _hold + _fadeOut;

    /// <summary>The message shown last; formatted once when shown.</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>0 hidden to 1 fully shown.</summary>
    public float Opacity =>
        _elapsed < _fadeIn ? (float)(_elapsed / _fadeIn)
        : _elapsed < _fadeIn + _hold ? 1
        : Math.Max(0, 1 - (float)((_elapsed - _fadeIn - _hold) / _fadeOut));

    /// <summary>Shows <paramref name="text"/> from the start of its fade-in.</summary>
    public void Show(string text)
    {
        Text = text;
        _elapsed = TimeSpan.Zero;
    }

    public void Update(TimeSpan delta) => _elapsed += delta;
}
