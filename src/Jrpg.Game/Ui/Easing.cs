namespace Jrpg.Game.Ui;

/// <summary>How far an animation has come, and how it eases, so every timed UI motion reads the same way.</summary>
internal static class Easing
{
    /// <summary>The share of <paramref name="length"/> that <paramref name="elapsed"/> covers, from 0 to 1; <paramref name="length"/> must be above zero.</summary>
    public static float ShareOf(TimeSpan elapsed, TimeSpan length) => Math.Clamp((float)(elapsed / length), 0, 1);

    /// <summary>Fast at first, settling gently at the end: 1 - (1 - share)³.</summary>
    public static float CubicOut(float share)
    {
        var remaining = 1 - share;
        return 1 - (remaining * remaining * remaining);
    }
}
