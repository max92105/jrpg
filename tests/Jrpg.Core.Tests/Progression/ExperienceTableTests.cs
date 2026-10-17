using Jrpg.Core.Progression;

namespace Jrpg.Core.Tests.Progression;

public sealed class ExperienceTableTests
{
    private readonly ExperienceTable _table = new([100, 120]);

    [Fact]
    public void ExperienceToReachNext_Level1_ReturnsFirstEntry()
    {
        var experience = _table.ExperienceToReachNext(1);

        Assert.Equal(100, experience);
    }

    [Fact]
    public void ExperienceToReachNext_MaxLevel_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _table.ExperienceToReachNext(3));
    }

    [Fact]
    public void IsMaxLevel_LevelPastLastEntry_ReturnsTrue()
    {
        var isMaxLevel = _table.IsMaxLevel(3);

        Assert.True(isMaxLevel);
    }

    [Fact]
    public void Constructor_SourceChangedAfterwards_KeepsOriginalEntries()
    {
        int[] source = [100, 120];
        var table = new ExperienceTable(source);

        source[0] = 5;

        Assert.Equal(100, table.ExperienceToReachNext(1));
    }

    [Fact]
    public void IsMaxLevel_LevelBelowMax_ReturnsFalse()
    {
        var isMaxLevel = _table.IsMaxLevel(2);

        Assert.False(isMaxLevel);
    }
}
