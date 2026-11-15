namespace Jrpg.Core.Cinematics;

/// <summary>
/// A still's opacity at a time into it (decision 629): rising from black over the fade, holding, falling over the fade,
/// so stills "appear and disappear" (decision 159). The game and the editor's preview read it, so a still fades the
/// same way in the game as in the editor.
/// </summary>
public static class StillFade
{
    /// <summary>
    /// How long a still takes to fade in, and again to fade out (placeholder); at most half its time.
    /// </summary>
    public static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(0.8);

    /// <returns>0 to 1: 0 at the start and from the end on, 1 while it holds.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="intoStill"/> is negative.</exception>
    public static float OpacityAt(TimeSpan intoStill, TimeSpan duration)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(intoStill, TimeSpan.Zero);
        var fade = TimeSpan.FromTicks(Math.Min(FadeTime.Ticks, duration.Ticks / 2));
        if (fade == TimeSpan.Zero)
        {
            return intoStill >= duration ? 0 : 1;
        }

        var fadingIn = intoStill / fade;
        var fadingOut = (duration - intoStill) / fade;
        return (float)Math.Clamp(Math.Min(fadingIn, fadingOut), 0, 1);
    }
}
