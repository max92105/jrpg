using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Editor.Party;

namespace Jrpg.Editor.Tests.Party;

// A class's numbers at every level of the draft's table (decision 948), read through Core's ClassSheet, and the stat
// whose number at the max level would pass a whole number's bounds, shown instead of wrapped rows.
public sealed class ClassLevelPreviewTests
{
    // Max level 4; each entry only it has, so a row reading the wrong one shows.
    private static readonly GameRules _rules =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")).Rules with
        {
            Experience = new ExperienceTable([30, 50, 70]),
        };

    private static readonly ClassStats _base =
        new(MaxHp: 140, Speed: 9) { Accuracy = 3, Primaries = new PrimaryStats(11, 7, 17, 5, 13) };

    private static readonly ClassStats _growth =
        new(MaxHp: 12, Speed: 2) { Accuracy = 4, Primaries = new PrimaryStats(6, 8, 10, 14, 16) };

    [Fact]
    public void RowsOf_TableOfThree_GivesFourLevels() =>
        Assert.Equal([1, 2, 3, 4], ClassLevelPreview.RowsOf(_base, _growth, _rules).Select(row => row.Level));

    [Fact]
    public void RowsOf_LastLevel_HasNoExperienceToNext() =>
        Assert.Null(ClassLevelPreview.RowsOf(_base, _growth, _rules)[^1].ExperienceToNext);

    [Fact]
    public void RowsOf_FirstLevel_ReadsTheFirstEntry() =>
        Assert.Equal(30, ClassLevelPreview.RowsOf(_base, _growth, _rules)[0].ExperienceToNext);

    [Fact]
    public void RowsOf_LevelThree_IsTheClassSheetAtThree()
    {
        var sheet = ClassSheet.At(new ClassDefinition("", "", "", _base, _growth), 3, _rules.Stats);

        var row = ClassLevelPreview.RowsOf(_base, _growth, _rules)[2];

        var accuracyPercent = StatFormula.WholePercentOf(sheet.AccuracyPermille);
        Assert.Equal(new LevelPreviewRow(3, 70, sheet.MaxHp, sheet.Speed, accuracyPercent, sheet.Primaries), row);
    }

    [Fact]
    public void RowsOf_GrowthPastTheBound_Throws() =>
        Assert.Throws<InvalidOperationException>(() => ClassLevelPreview.RowsOf(_base, WithPower(int.MaxValue), _rules));

    [Fact]
    public void ProblemOf_GrowthIntMax_NamesTheStat() =>
        Assert.Equal(
            "Growth power is too large: level 4 would pass 2147483647.",
            ClassLevelPreview.ProblemOf(WithPower(10), WithPower(int.MaxValue), maxLevel: 4));

    // 1 + 715827882 x 3 is exactly int.MaxValue; a base of 2 passes it by one.
    [Fact]
    public void ProblemOf_LargestThatFits_IsNull() =>
        Assert.Null(ClassLevelPreview.ProblemOf(WithPower(1), WithPower(715827882), maxLevel: 4));

    [Fact]
    public void ProblemOf_OnePastTheLargest_NamesTheStat() =>
        Assert.NotNull(ClassLevelPreview.ProblemOf(WithPower(2), WithPower(715827882), maxLevel: 4));

    // -2 - 715827882 x 3 is exactly int.MinValue; a base of -3 passes it by one.
    [Fact]
    public void ProblemOf_SmallestThatFits_IsNull() =>
        Assert.Null(ClassLevelPreview.ProblemOf(WithPower(-2), WithPower(-715827882), maxLevel: 4));

    [Fact]
    public void ProblemOf_OneBelowTheSmallest_NamesTheStat() =>
        Assert.Equal(
            "Growth power is too small: level 4 would pass -2147483648.",
            ClassLevelPreview.ProblemOf(WithPower(-3), WithPower(-715827882), maxLevel: 4));

    [Fact]
    public void ProblemOf_TwoStatsPass_NamesTheFirstInThePagesOrder()
    {
        var growth = new ClassStats(MaxHp: int.MaxValue, Speed: 0) { Primaries = PrimaryStats.None with { Will = int.MaxValue } };

        var problem = ClassLevelPreview.ProblemOf(_base, growth, maxLevel: 4);

        Assert.StartsWith("Growth max HP is too large", problem);
    }

    [Fact]
    public void ProblemOf_MaxLevelOne_IsNull() =>
        Assert.Null(ClassLevelPreview.ProblemOf(_base, WithPower(int.MaxValue), maxLevel: 1));

    private static ClassStats WithPower(int power) =>
        new(MaxHp: 1, Speed: 0) { Primaries = PrimaryStats.None with { Power = power } };
}
