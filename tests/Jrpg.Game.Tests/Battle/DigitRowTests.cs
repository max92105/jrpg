using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// Each digit's place along a number's row; advances indexed by digit value, here 1 = 20 px, 2 = 30 px, 7 = 25 px.
public sealed class DigitRowTests
{
    private static readonly float[] _advances = [10, 20, 30, 10, 10, 10, 10, 25, 10, 10];

    [Fact]
    public void Of_TwoDigits_PutsTheSecondAfterTheFirstsAdvance()
    {
        var row = DigitRow.Of("12", _advances, -2);

        Assert.Equal((0f, 18f), (row.LeftOf(0), row.LeftOf(1)));
    }

    [Fact]
    public void Width_ThreeDigits_IsTheAdvancesLessTheSpacing()
    {
        var row = DigitRow.Of("127", _advances, -2);

        Assert.Equal(20 + 30 + 25 - 4, row.Width);
    }

    [Fact]
    public void Of_RepeatedDigit_UsesItsAdvanceTwice()
    {
        var row = DigitRow.Of("221", _advances, -2);

        Assert.Equal((28f, 56f), (row.LeftOf(1), row.LeftOf(2)));
    }
}
