namespace Jrpg.Game.Hosting;

/// <summary>A volume setting's eleven steps (decision 424) read as a gain.</summary>
internal static class VolumeLevel
{
    public const int Max = 10;

    /// <returns>
    /// The gain, 0 to 1: <c>(level / 10)²</c>, a placeholder taper, since loudness isn't linear in gain and a straight
    /// line would leave the low steps all loud.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">The level is below 0 or above <see cref="Max"/>.</exception>
    public static float GainOf(int level)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(level);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(level, Max);
        var share = level / (float)Max;
        return share * share;
    }
}
