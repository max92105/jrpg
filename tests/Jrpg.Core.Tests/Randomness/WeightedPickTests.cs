using Jrpg.Core.Randomness;

namespace Jrpg.Core.Tests.Randomness;

// A scripted random with no values throws on any draw, so a test without values proves nothing was drawn.
public sealed class WeightedPickTests
{
    [Fact]
    public void Pick_OneWeightHoldsAll_DrawsNothing()
    {
        var index = WeightedPick.Pick([0, 7, 0], new FixedSequenceRandom());

        Assert.Equal(1, index);
    }

    [Fact]
    public void Pick_DrawBelowFirstWeight_PicksFirst()
    {
        var index = WeightedPick.Pick([650, 280, 70], new FixedSequenceRandom(649));

        Assert.Equal(0, index);
    }

    [Fact]
    public void Pick_DrawAtFirstWeight_PicksSecond()
    {
        var index = WeightedPick.Pick([650, 280, 70], new FixedSequenceRandom(650));

        Assert.Equal(1, index);
    }

    // The draw is the zero weight's start, which is also the next weight's: the next one takes it.
    [Fact]
    public void Pick_ZeroWeight_NeverPicked()
    {
        var index = WeightedPick.Pick([5, 0, 5], new FixedSequenceRandom(5));

        Assert.Equal(2, index);
    }

    [Fact]
    public void Pick_EveryWeightZero_Throws()
    {
        Assert.Throws<ArgumentException>(() => WeightedPick.Pick([0, 0], new FixedSequenceRandom()));
    }
}
