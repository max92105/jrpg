using Jrpg.Editor.Progression;
using static Jrpg.Editor.Tests.Progression.ExperienceFixtures;

namespace Jrpg.Editor.Tests.Progression;

// The Experience page's table (decision 956): a row per level step with its running total, a level added or the last
// removed, the typed boxes' problems first, then Data's, and the note on a lowered max level; one record, so its host
// has nothing to choose.
public sealed class ExperienceTableViewModelTests
{
    private readonly ExperienceTableViewModel _table = TableOf(TestTable);

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
        var table = TableOf(new([int.MaxValue, int.MaxValue]));

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
    public void AddLevel_CopiesTheLastAmount()
    {
        _table.AddLevelCommand.Execute(null);

        var added = _table.Rows[3];
        Assert.Equal(("Level 4 to 5", "40", 110L), (added.Label, added.AmountText, added.TotalToReach));
        Assert.Equal(5, _table.MaxLevel);
    }

    [Fact]
    public void AddLevel_RowTypedAfterwards_RaisesProblemChanged()
    {
        _table.AddLevelCommand.Execute(null);
        var raised = false;
        _table.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(ExperienceTableViewModel.Problem);

        _table.Rows[3].AmountText = "lots";

        Assert.True(raised);
    }

    [Fact]
    public void AddLevel_FromOneRow_LightsRemoveLastLevel()
    {
        var table = TableOf(new([10]));
        var raised = false;
        table.RemoveLastLevelCommand.CanExecuteChanged += (_, _) => raised = true;

        table.AddLevelCommand.Execute(null);

        Assert.True(raised && table.RemoveLastLevelCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveLastLevel_TakesTheLastOut()
    {
        _table.RemoveLastLevelCommand.Execute(null);

        Assert.Equal(["Level 1 to 2", "Level 2 to 3"], _table.Rows.Select(row => row.Label));
        Assert.Equal(3, _table.MaxLevel);
    }

    [Theory]
    [InlineData(nameof(ExperienceTableViewModel.MaxLevel))]
    [InlineData(nameof(ExperienceTableViewModel.Problem))]
    public void RemoveLastLevel_RaisesChanged(string property)
    {
        var raised = false;
        _table.PropertyChanged += (_, e) => raised |= e.PropertyName == property;

        _table.RemoveLastLevelCommand.Execute(null);

        Assert.True(raised);
    }

    [Fact]
    public void RemoveLastLevel_RemovedRowTyped_RaisesNothing()
    {
        var removed = _table.Rows[2];
        _table.RemoveLastLevelCommand.Execute(null);
        var raised = false;
        _table.PropertyChanged += (_, _) => raised = true;

        removed.AmountText = "lots";

        Assert.False(raised);
    }

    [Fact]
    public void RemoveLastLevel_OneRow_IsGreyed()
    {
        var table = TableOf(new([10]));

        Assert.False(table.RemoveLastLevelCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveLastLevel_TwoRows_IsLit()
    {
        var table = TableOf(new([10, 20]));

        Assert.True(table.RemoveLastLevelCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveLastLevel_ToOneRow_RaisesCanExecuteChanged()
    {
        var table = TableOf(new([10, 20]));
        var raised = false;
        table.RemoveLastLevelCommand.CanExecuteChanged += (_, _) => raised = true;

        table.RemoveLastLevelCommand.Execute(null);

        Assert.True(raised && !table.RemoveLastLevelCommand.CanExecute(null));
    }

    [Fact]
    public void Problem_ClassSkillPastNewMax_ShowsDataMessage()
    {
        _table.RemoveLastLevelCommand.Execute(null);

        Assert.Equal(
            "classes.json: class 'archer': skills lists 'expose' at level 4, past the max level 3.",
            _table.Problem);
    }

    [Fact]
    public void MaxLevelNote_FewerLevels_SaysTheDrop()
    {
        _table.RemoveLastLevelCommand.Execute(null);

        Assert.Equal("Max level 3, was 4: saved members above it drop to 3 when a save loads.", _table.MaxLevelNote);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void MaxLevelNote_SameOrMore_IsNull(int levelsAdded)
    {
        for (var added = 0; added < levelsAdded; added++)
        {
            _table.AddLevelCommand.Execute(null);
        }

        Assert.Null(_table.MaxLevelNote);
    }

    [Fact]
    public void MaxLevelNote_LevelAddedBack_ClearsAndRaisesChanged()
    {
        _table.RemoveLastLevelCommand.Execute(null);
        var raised = false;
        _table.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(ExperienceTableViewModel.MaxLevelNote);

        _table.AddLevelCommand.Execute(null);

        Assert.True(raised && _table.MaxLevelNote is null);
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
