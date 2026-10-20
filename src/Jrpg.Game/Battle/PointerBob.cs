namespace Jrpg.Game.Battle;

/// <summary>How far the target pointer has bobbed (decision 56): down 8 px over 0.45 s and back, eased like a sine.</summary>
internal static class PointerBob
{
    public const float Distance = 8;

    private static readonly TimeSpan _oneWay = TimeSpan.FromSeconds(0.45);

    /// <param name="elapsed">How long the bob has run; it repeats every 0.9 s.</param>
    /// <returns>0 at rest to <see cref="Distance"/> at the bottom of the bob, in pixels.</returns>
    public static float OffsetAt(TimeSpan elapsed)
    {
        var phase = elapsed / _oneWay * MathF.PI;
        return Distance * (1 - MathF.Cos((float)phase)) / 2;
    }
}
