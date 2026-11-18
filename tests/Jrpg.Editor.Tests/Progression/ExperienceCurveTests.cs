using Jrpg.Editor.Progression;

namespace Jrpg.Editor.Tests.Progression;

// The fill's rule (decision 957): each step the one before times (100 + growth) / 100, rounded down; refused outside 2
// to 99 levels (decision 958), at -100 % or below, below a first of 1, and at the first step past the largest whole
// number or below 1.
public sealed class ExperienceCurveTests
{
    [Fact]
    public void Rows_HundredAtTwentyPercent_GrowsAndRoundsDown()
    {
        Assert.Equal([100, 120, 144, 172, 206], ExperienceCurve.Rows(100, 20, 6));
    }

    [Fact]
    public void Rows_ZeroGrowth_RepeatsTheFirst()
    {
        Assert.Equal([75, 75, 75], ExperienceCurve.Rows(75, 0, 4));
    }

    [Fact]
    public void Rows_NegativeGrowth_Shrinks()
    {
        Assert.Equal([100, 90, 81], ExperienceCurve.Rows(100, -10, 4));
    }

    [Fact]
    public void Rows_MaxLevelTwo_GivesOneRow()
    {
        Assert.Equal([100], ExperienceCurve.Rows(100, 20, 2));
    }

    [Fact]
    public void Rows_WithAProblem_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ExperienceCurve.Rows(100, 20, 1));
    }

    [Fact]
    public void ProblemOf_MaxLevelOne_Refuses()
    {
        Assert.Equal("Max level must be 2 to 99 (was 1).", ExperienceCurve.ProblemOf(100, 20, 1));
    }

    [Fact]
    public void ProblemOf_MaxLevel99_IsNull()
    {
        Assert.Null(ExperienceCurve.ProblemOf(1, 0, 99));
    }

    [Fact]
    public void ProblemOf_MaxLevel100_Refuses()
    {
        Assert.Equal("Max level must be 2 to 99 (was 100).", ExperienceCurve.ProblemOf(1, 0, 100));
    }

    [Fact]
    public void ProblemOf_GrowthMinus100_Refuses()
    {
        Assert.Equal("Growth must be above -100 % (was -100).", ExperienceCurve.ProblemOf(100, -100, 2));
    }

    // At -99 % a first of 100 still makes 1 on the second step, the smallest amount the game takes.
    [Fact]
    public void ProblemOf_GrowthMinus99_IsNull()
    {
        Assert.Null(ExperienceCurve.ProblemOf(100, -99, 3));
    }

    [Fact]
    public void ProblemOf_FirstZero_Refuses()
    {
        Assert.Equal("First must be at least 1 (was 0).", ExperienceCurve.ProblemOf(0, 20, 5));
    }

    [Fact]
    public void ProblemOf_RowPastIntMax_NamesTheLevelStep()
    {
        Assert.Equal(
            "Level 2 to 3 would need more than 2147483647.",
            ExperienceCurve.ProblemOf(int.MaxValue / 2, 300, 4));
    }

    [Fact]
    public void ProblemOf_RowBelowOne_NamesTheLevelStep()
    {
        Assert.Equal("Level 3 to 4 would need less than 1.", ExperienceCurve.ProblemOf(5, -60, 5));
    }

    [Fact]
    public void ProblemOf_FirstIntMaxGrowthZero_IsNull()
    {
        Assert.Null(ExperienceCurve.ProblemOf(int.MaxValue, 0, 99));
    }

    // The growth added to 100 in a long: as an int it would wrap to a negative factor and pass as a shrink.
    [Fact]
    public void ProblemOf_GrowthIntMax_NamesTheSecondStep()
    {
        Assert.Equal(
            "Level 2 to 3 would need more than 2147483647.",
            ExperienceCurve.ProblemOf(100, int.MaxValue, 3));
    }
}
