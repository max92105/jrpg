using Jrpg.Core.Randomness;

namespace Jrpg.Core.Tests.Randomness;

public sealed class RangeRollTests
{
    [Fact]
    public void Within_OneValue_DrawsNothing()
    {
        var value = RangeRoll.Within(3, 3, new FixedSequenceRandom());

        Assert.Equal(3, value);
    }

    // 2..5 holds four values, so the highest draw is 3 and gives the top of the range.
    [Fact]
    public void Within_HighestDraw_TakesMax()
    {
        var value = RangeRoll.Within(2, 5, new FixedSequenceRandom(3));

        Assert.Equal(5, value);
    }
}
