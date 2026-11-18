using Jrpg.Editor.Progression;
using static Jrpg.Editor.Tests.Progression.ExperienceFixtures;

namespace Jrpg.Editor.Tests.Progression;

// The Experience page's table (decision 956): a row per level step with its running total, the typed boxes' problems
// first, then Data's; one record, so its host has nothing to choose.
public sealed class ExperienceTableViewModelTests
{
    private readonly ExperienceTableViewModel _table = new(World);

    [Fact]
    public void Constructor_Table_ListsOneRowPerStep()
    {
        Assert.Equal(
            [("Level 1 to 2", "10"), ("Level 2 to 3", "20"), ("Level 3 to 4", "40")],
            _table.Rows.Select(row => (row.Label, row.AmountText)));
        Assert.Equal(4, _table.MaxLevel);
    }

    [Fact]
    public void TotalToReach_SumsTheRowsAbove()
    {
        Assert.Equal([10L, 30L, 70L], _table.Rows.Select(row => row.TotalToReach));
    }

    [Fact]
    public void TotalToReach_BoxAboveNotANumber_IsBlank()
    {
        _table.Rows[1].AmountText = "lots";

        Assert.Equal([10L, null, null], _table.Rows.Select(row => row.TotalToReach));
    }

    [Fact]
    public void TotalToReach_TypedAmount_UpdatesTheRowsBelow()
    {
        _table.Rows[0].AmountText = "15";

        Assert.Equal([15L, 35L, 75L], _table.Rows.Select(row => row.TotalToReach));
    }

    [Fact]
    public void TotalToReach_RowsNearIntMax_DoesNotOverflow()
    {
        var table = new ExperienceTableViewModel(WithTable(new([int.MaxValue, int.MaxValue])));

        Assert.Equal(2L * int.MaxValue, table.Rows[1].TotalToReach);
    }

    [Fact]
    public void Problem_RowNotANumber_NamesTheLevelStep()
    {
        _table.Rows[1].AmountText = "ten";

        Assert.Equal("Level 2 to 3 must be a whole number (was 'ten').", _table.Problem);
    }

    [Fact]
    public void Problem_TwoRowsNotNumbers_NamesTheFirst()
    {
        (_table.Rows[1].AmountText, _table.Rows[2].AmountText) = ("ten", "");

        Assert.Equal("Level 2 to 3 must be a whole number (was 'ten').", _table.Problem);
    }

    [Fact]
    public void Problem_ZeroAmount_ShowsDataMessage()
    {
        _table.Rows[2].AmountText = "0";

        Assert.Equal("rules.json: experience.experienceToNextLevel[2] must be at least 1 (was 0).", _table.Problem);
    }

    [Fact]
    public void Problem_AmountOne_IsNull()
    {
        _table.Rows[2].AmountText = "1";

        Assert.Null(_table.Problem);
    }

    [Fact]
    public void Problem_TypedAmount_RaisesProblemChanged()
    {
        var raised = false;
        _table.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(ExperienceTableViewModel.Problem);

        _table.Rows[0].AmountText = "11";

        Assert.True(raised);
    }

    [Fact]
    public void ToTable_TypedAmount_SetsIt()
    {
        _table.Rows[1].AmountText = " 25 ";

        Assert.Equal([10, 25, 40], _table.ToTable().ExperienceToNextLevel);
    }

    [Fact]
    public void ToTable_WithAProblem_Throws()
    {
        _table.Rows[0].AmountText = "many";

        Assert.Throws<InvalidOperationException>(_table.ToTable);
    }

    [Fact]
    public void ChosenKey_IsNull()
    {
        Assert.Null(_table.ChosenKey);
    }

    [Fact]
    public void Choose_AnyKey_ChangesNothing()
    {
        _table.Choose("experience");

        Assert.Equal(["10", "20", "40"], _table.Rows.Select(row => row.AmountText));
    }
}
