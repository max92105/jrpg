using Jrpg.Editor.Progression;
using static Jrpg.Editor.Tests.Progression.ExperienceFixtures;

namespace Jrpg.Editor.Tests.Progression;

// The Experience page's fill from a curve (decision 957): the boxes open on the table, Fill greys on their problem and
// replaces the rows, which stay watched and editable.
public sealed class ExperienceTableFillTests
{
    private readonly ExperienceTableViewModel _table = TableOf(TestTable);

    [Fact]
    public void Curve_OpensOnTheTablesFirstAndMaxLevel()
    {
        Assert.Equal(("10", "20", "4"), (_table.Curve.FirstText, _table.Curve.GrowthText, _table.Curve.MaxLevelText));
    }

    [Fact]
    public void Fill_ValidCurve_ReplacesTheRows()
    {
        (_table.Curve.FirstText, _table.Curve.GrowthText, _table.Curve.MaxLevelText) = ("100", "20", "6");

        _table.FillCommand.Execute(null);

        Assert.Equal(["100", "120", "144", "172", "206"], _table.Rows.Select(row => row.AmountText));
        Assert.Equal("Level 5 to 6", _table.Rows[4].Label);
    }

    [Fact]
    public void Fill_RecomputesTotalsAndNote()
    {
        (_table.Curve.FirstText, _table.Curve.GrowthText, _table.Curve.MaxLevelText) = ("5", "100", "3");

        _table.FillCommand.Execute(null);

        Assert.Equal([5L, 15L], _table.Rows.Select(row => row.TotalToReach));
        Assert.Equal("Max level 3, was 4: saved members above it drop to 3 when a save loads.", _table.MaxLevelNote);
    }

    [Fact]
    public void Fill_ReplacedRowTyped_RaisesNothing()
    {
        var replaced = _table.Rows[0];
        _table.FillCommand.Execute(null);
        var raised = false;
        _table.PropertyChanged += (_, _) => raised = true;

        replaced.AmountText = "lots";

        Assert.False(raised);
    }

    [Fact]
    public void Fill_FilledRowTyped_RaisesProblemChanged()
    {
        _table.FillCommand.Execute(null);
        var raised = false;
        _table.PropertyChanged += (_, e) => raised |= e.PropertyName == nameof(ExperienceTableViewModel.Problem);

        _table.Rows[0].AmountText = "lots";

        Assert.True(raised);
    }

    [Theory]
    [InlineData("ten", "20", "4", "First must be a whole number (was 'ten').")]
    [InlineData("10", "", "4", "Growth must be a whole number (was '').")]
    [InlineData("10", "20", "x", "Max level must be a whole number (was 'x').")]
    [InlineData("10", "20", "100", "Max level must be 2 to 99 (was 100).")]
    public void Fill_WithProblem_IsGreyed(string first, string growth, string maxLevel, string problem)
    {
        (_table.Curve.FirstText, _table.Curve.GrowthText, _table.Curve.MaxLevelText) = (first, growth, maxLevel);

        Assert.Equal(problem, _table.Curve.Problem);
        Assert.False(_table.FillCommand.CanExecute(null));
    }

    [Fact]
    public void Fill_NoProblem_IsLit()
    {
        Assert.True(_table.FillCommand.CanExecute(null));
    }

    [Fact]
    public void Fill_CurveBoxTyped_RaisesCanExecuteChanged()
    {
        var raised = false;
        _table.FillCommand.CanExecuteChanged += (_, _) => raised = true;

        _table.Curve.MaxLevelText = "100";

        Assert.True(raised);
    }
}
