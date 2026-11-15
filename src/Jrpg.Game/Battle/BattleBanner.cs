namespace Jrpg.Game.Battle;

/// <summary>
/// The plaque naming the action (decision 646): it pops in at full opacity, stays 1.5 s and vanishes at once, as the
/// reference clip shows it (no fade at its 33 ms frames). Pure timing; <see cref="BattleBannerView"/> draws it.
/// </summary>
internal sealed class BattleBanner
{
    // How long the plaque shows (the clip's).
    private static readonly TimeSpan _showFor = TimeSpan.FromSeconds(1.5);

    // Starts past the end, so nothing shows until the first message.
    private TimeSpan _elapsed = _showFor;

    /// <summary>The message shown last; formatted once when shown.</summary>
    public string Text { get; private set; } = string.Empty;

    /// <summary>1 while shown, 0 hidden: the plaque pops in and out.</summary>
    public float Opacity => _elapsed < _showFor ? 1 : 0;

    /// <summary>Shows <paramref name="text"/> from now.</summary>
    public void Show(string text)
    {
        Text = text;
        _elapsed = TimeSpan.Zero;
    }

    public void Update(TimeSpan delta) => _elapsed += delta;
}
