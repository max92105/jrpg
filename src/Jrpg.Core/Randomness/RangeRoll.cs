namespace Jrpg.Core.Randomness;

/// <summary>
/// A whole number within a range, both ends included, drawn only when the range holds more than one (ADR 0026).
/// </summary>
internal static class RangeRoll
{
    /// <param name="min">The lowest result.</param>
    /// <param name="max">The highest result, at least <paramref name="min"/>.</param>
    /// <param name="random">Drawn from once, unless <paramref name="min"/> equals <paramref name="max"/>.</param>
    public static int Within(int min, int max, IRandom random) =>
        min < max ? min + random.NextBelow(max - min + 1) : min;
}
