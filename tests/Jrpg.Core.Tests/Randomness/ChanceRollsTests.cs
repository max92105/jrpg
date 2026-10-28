using Jrpg.Core.Randomness;

namespace Jrpg.Core.Tests.Randomness;

// A scripted random with no values throws on any draw, so these prove a certain outcome draws nothing.
public sealed class ChanceRollsTests
{
    [Fact]
    public void Succeeds_Thousand_DrawsNothing()
    {
        var rolls = new ChanceRolls(new FixedSequenceRandom());

        Assert.True(rolls.Succeeds(1000));
    }

    [Fact]
    public void Succeeds_Zero_DrawsNothing()
    {
        var rolls = new ChanceRolls(new FixedSequenceRandom());

        Assert.False(rolls.Succeeds(0));
    }

    [Fact]
    public void Succeeds_FiveHundred_DrawsOnceAndSucceedsBelowIt()
    {
        var rolls = new ChanceRolls(new FixedSequenceRandom(499));

        Assert.True(rolls.Succeeds(500));
    }

    [Fact]
    public void Succeeds_FiveHundred_FailsFromIt()
    {
        var rolls = new ChanceRolls(new FixedSequenceRandom(500));

        Assert.False(rolls.Succeeds(500));
    }
}
