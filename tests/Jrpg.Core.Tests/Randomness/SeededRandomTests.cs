using Jrpg.Core.Randomness;

namespace Jrpg.Core.Tests.Randomness;

public sealed class SeededRandomTests
{
    // Pinned values: if a .NET update changes the seeded Random sequence, every seeded battle and simulation changes too.
    [Fact]
    public void NextBelow_Seed42_ReturnsKnownSequence()
    {
        var random = new SeededRandom(42);

        var values = Enumerable.Range(0, 5).Select(_ => random.NextBelow(100)).ToList();

        Assert.Equal([66, 14, 12, 52, 16], values);
    }
}
