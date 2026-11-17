using Jrpg.Core.Content;
using Jrpg.Core.Progression;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// The Levels tab's rows as one class types them (decision 948): read from the boxes as typed against the draft's table,
// or empty with the problem that keeps them from showing.
public sealed class ClassViewModelLevelTests
{
    // A table of two entries built here, max level 3, so the rows can only come from the content handed in.
    private static readonly ContentCatalog _threeLevels =
        World with { Rules = World.Rules with { Experience = new ExperienceTable([30, 50]) } };

    [Fact]
    public void LevelRows_Untouched_ReadsTheDraftsTable()
    {
        var rows = ClassViewModel.From(Scholar, _threeLevels).LevelRows;

        int?[] experienceToNext = [30, 50, null];
        Assert.Equal([1, 2, 3], rows.Select(row => row.Level));
        Assert.Equal(experienceToNext, rows.Select(row => row.ExperienceToNext));
    }

    // The scholar's growth power is 6: rising by 5 shows the typed box was read.
    [Fact]
    public void LevelRows_TypedGrowth_FollowsIt()
    {
        var scholar = ClassViewModel.From(Scholar, _threeLevels);

        scholar.Growth.PowerText = "5";

        Assert.Equal([11, 16, 21], scholar.LevelRows.Select(row => row.Primaries.Power));
        Assert.Null(scholar.LevelsProblem);
    }

    [Fact]
    public void LevelRows_BoxNotANumber_IsEmptyWithProblem()
    {
        var scholar = ClassViewModel.From(Scholar, _threeLevels);

        scholar.Base.SpeedText = "x";

        Assert.Empty(scholar.LevelRows);
        Assert.Equal("Base speed must be a whole number (was 'x').", scholar.LevelsProblem);
    }

    [Fact]
    public void LevelsProblem_GrowthBoxNotANumber_NamesIt()
    {
        var scholar = ClassViewModel.From(Scholar, _threeLevels);

        scholar.Growth.WillText = "x";

        Assert.Equal("Growth will must be a whole number (was 'x').", scholar.LevelsProblem);
    }

    [Fact]
    public void LevelRows_GrowthPastTheBound_IsEmptyWithProblem()
    {
        var scholar = ClassViewModel.From(Scholar, _threeLevels);

        scholar.Growth.PowerText = "2147483647";

        Assert.Empty(scholar.LevelRows);
        Assert.Equal("Growth power is too large: level 3 would pass 2147483647.", scholar.LevelsProblem);
    }

    [Fact]
    public void LevelRows_NewClass_ReadsTheDraftsTable() =>
        Assert.Equal([1, 2, 3], ClassViewModel.New(_threeLevels).LevelRows.Select(row => row.Level));

    [Fact]
    public void LevelRows_Duplicate_ReadsTheChosensTable()
    {
        var chosen = ClassViewModel.From(Scholar, _threeLevels);

        var copy = ClassViewModel.DuplicateOf(chosen);

        Assert.Equal(chosen.LevelRows, copy.LevelRows);
    }

    [Fact]
    public void LevelRows_CatalogsClass_ReadsTheCatalogsContent() =>
        Assert.Equal([1, 2, 3], new ClassCatalogViewModel(_threeLevels).SelectedClass!.LevelRows.Select(row => row.Level));

    [Fact]
    public void LevelRows_CatalogsAddedClass_ReadsTheCatalogsContent()
    {
        var catalog = new ClassCatalogViewModel(_threeLevels);

        catalog.AddClassCommand.Execute(null);

        Assert.Equal([1, 2, 3], catalog.SelectedClass!.LevelRows.Select(row => row.Level));
    }
}
