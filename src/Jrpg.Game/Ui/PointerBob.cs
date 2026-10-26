namespace Jrpg.Game.Ui;

/// <summary>
/// How far a pointer has bobbed: out over 0.45 s and back, eased like a sine. The battle's target pointer bobs down 8
/// px (decision 56); a list's pointer can bob its own distance.
/// </summary>
internal static class PointerBob
{
    private static readonly TimeSpan _oneWay = TimeSpan.FromSeconds(0.45);

    /// <param name="elapsed">How long the bob has run; it repeats every 0.9 s.</param>
    /// <param name="distance">The full bob, in pixels.</param>
    /// <returns>0 at rest to <paramref name="distance"/> at the far end of the bob, in pixels.</returns>
    public static float OffsetAt(TimeSpan elapsed, float distance)
    {
        var phase = elapsed / _oneWay * MathF.PI;
        return distance * (1 - MathF.Cos((float)phase)) / 2;
    }
}
