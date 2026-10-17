namespace Jrpg.Core.Randomness;

/// <summary>
/// <see cref="IRandom"/> over <see cref="Random"/>. A seeded <see cref="Random"/> has kept the same sequence
/// across .NET versions so far, but .NET doesn't guarantee it; SeededRandomTests pins known values to catch a change.
/// </summary>
public sealed class SeededRandom(int seed) : IRandom
{
    private readonly Random _random = new(seed);

    public int NextBelow(int maxExclusive) => _random.Next(maxExclusive);
}
