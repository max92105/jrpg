namespace Jrpg.Core.Randomness;

/// <summary>
/// Picks one of several outcomes by weight with a single draw: each one's chance is its weight over the total (ADR
/// 0030). A certain outcome draws nothing (ADR 0026), so a table with one tier left keeps a seeded sequence unchanged.
/// </summary>
internal static class WeightedPick
{
    /// <param name="weights">At least 0 each, at least one above 0; a weight of 0 is never picked.</param>
    /// <param name="random">Drawn from once, unless one weight holds the whole total.</param>
    /// <returns>The picked weight's index.</returns>
    /// <exception cref="ArgumentException">Every weight is 0.</exception>
    public static int Pick(IReadOnlyList<int> weights, IRandom random)
    {
        var total = weights.Sum();
        if (total <= 0)
        {
            throw new ArgumentException("At least one weight must be above 0.", nameof(weights));
        }

        var draw = weights.Contains(total) ? 0 : random.NextBelow(total);
        for (var index = 0; index < weights.Count; index++)
        {
            if (draw < weights[index])
            {
                return index;
            }

            draw -= weights[index];
        }

        throw new InvalidOperationException("A draw below the total always lands on a weight.");
    }
}
