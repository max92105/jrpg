using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Maps;

public sealed class MapFolderPathTests
{
    private const string Rule = "A folder path is names joined by '/', each with text, no space at either end and no backslash.";

    [Fact]
    public void ProblemOf_TwoNames_IsNull() => Assert.Null(MapFolderPath.ProblemOf("Act 1/Village"));

    [Fact]
    public void ProblemOf_EmptyName_NamesTheRule() => Assert.Equal(Rule, MapFolderPath.ProblemOf("Act 1//Village"));

    [Fact]
    public void ProblemOf_TrailingSeparator_NamesTheRule() => Assert.Equal(Rule, MapFolderPath.ProblemOf("Act 1/"));

    [Fact]
    public void ProblemOf_SpaceAtAnEnd_NamesTheRule() => Assert.Equal(Rule, MapFolderPath.ProblemOf("Act 1/Village "));

    [Fact]
    public void ProblemOf_Backslash_NamesTheRule() => Assert.Equal(Rule, MapFolderPath.ProblemOf(@"Act 1\Village"));

    [Fact]
    public void ProblemOf_Empty_NamesTheRule() => Assert.Equal(Rule, MapFolderPath.ProblemOf(""));

    [Fact]
    public void NamesOf_Nested_GivesEachName() => Assert.Equal(["Act 1", "Village"], MapFolderPath.NamesOf("Act 1/Village"));

    [Fact]
    public void IsWithin_Subfolder_IsTrue() => Assert.True(MapFolderPath.IsWithin("Act 1/Village", "Act 1"));

    [Fact]
    public void IsWithin_SameStartLongerName_IsFalse() => Assert.False(MapFolderPath.IsWithin("Act 10", "Act 1"));

    [Fact]
    public void IsWithin_Itself_IsTrue() => Assert.True(MapFolderPath.IsWithin("Act 1", "Act 1"));

    [Fact]
    public void IsWithin_ParentFolder_IsFalse() => Assert.False(MapFolderPath.IsWithin("Act 1", "Act 1/Village"));

    [Fact]
    public void Moved_Subfolder_FollowsTheNewRoot() => Assert.Equal("Chapter 1/Village", MapFolderPath.Moved("Act 1/Village", "Act 1", "Chapter 1"));

    [Fact]
    public void Moved_TheFolderItself_IsTheNewPath() => Assert.Equal("Act 2/Woods", MapFolderPath.Moved("Act 1/Woods", "Act 1/Woods", "Act 2/Woods"));
}
