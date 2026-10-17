using Jrpg.Core.Randomness;

namespace Jrpg.Core.Tests.Randomness;

/// <summary>Returns scripted values in order, so a test can force exactly what "random" code does.</summary>
internal sealed class FixedSequenceRandom(params int[] values) : IRandom
{
    private int _nextIndex;

    public int NextBelow(int maxExclusive)
    {
        if (_nextIndex >= values.Length)
        {
            throw new InvalidOperationException($"The test scripted {values.Length} random values but more were asked for.");
        }

        var value = values[_nextIndex++];
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, maxExclusive);
        return value;
    }
}
