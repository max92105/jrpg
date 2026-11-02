namespace Jrpg.Game.Dialogue;

/// <summary>
/// Whether a page's typing ticks this update (decision 422): every second character typed, never for a space, and at
/// least 60 ms after the last tick, so a fast speed doesn't buzz.
/// </summary>
internal static class TypingTicks
{
    public const int CharactersPerTick = 2;

    public static readonly TimeSpan MinimumGap = TimeSpan.FromMilliseconds(60);

    /// <param name="page">The page typing out.</param>
    /// <param name="visibleBefore">How many of its characters showed before this update.</param>
    /// <param name="visibleNow">How many show now.</param>
    /// <param name="sinceLastTick">The time since the last tick.</param>
    /// <returns>True at most once per update, however many characters this update typed.</returns>
    public static bool IsDue(string page, int visibleBefore, int visibleNow, TimeSpan sinceLastTick)
    {
        var lastMultiple = visibleNow / CharactersPerTick * CharactersPerTick;
        return lastMultiple > visibleBefore
            && sinceLastTick >= MinimumGap
            && !char.IsWhiteSpace(page[lastMultiple - 1]);
    }
}
