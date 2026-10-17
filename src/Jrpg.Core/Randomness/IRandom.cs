namespace Jrpg.Core.Randomness;

/// <summary>The only source of randomness in Core, injected so tests and simulations can fix the seed.</summary>
public interface IRandom
{
    /// <param name="maxExclusive">At least 1.</param>
    /// <returns>A number in 0..<paramref name="maxExclusive"/> - 1.</returns>
    int NextBelow(int maxExclusive);
}
