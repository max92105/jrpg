using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class TilesheetIdsTests
{
    [Theory]
    [InlineData("Grass3.png", "grass_3")]
    [InlineData("tree02_s_01_animation.png", "tree_02_s_01_animation")]
    [InlineData("RA_Jungle.png", "ra_jungle")]
    [InlineData("FG_Forest_Fall.png", "fg_forest_fall")]
    [InlineData("DarkCave.png", "dark_cave")]
    public void FromFile_MixedCaseWithDigits_IsSnakeCase(string file, string expected)
    {
        Assert.Equal(expected, TilesheetIds.FromFile(file, []));
    }

    [Fact]
    public void FromFile_PackNameWithSpacesAndBrackets_CollapsesUnderscores()
    {
        Assert.Equal("autumn_trees_bare_80_x_112", TilesheetIds.FromFile("autumn trees (bare) 80x112.png", []));
    }

    [Fact]
    public void FromFile_NameEndingInABracket_HasNoTrailingUnderscore()
    {
        Assert.Equal("autumn_forest_clean", TilesheetIds.FromFile("autumn forest (clean).png", []));
    }

    [Fact]
    public void FromFile_NameStartingWithABracket_HasNoLeadingUnderscore()
    {
        Assert.Equal("bare_tree", TilesheetIds.FromFile("(bare) tree.png", []));
    }

    // The catalog save refuses an empty id and the Cut panel can't edit one, so the name gets a word.
    [Fact]
    public void FromFile_NoLetterOrDigit_IsSheet()
    {
        Assert.Equal("sheet", TilesheetIds.FromFile("---.png", []));
    }

    [Fact]
    public void FromFile_IdTaken_AppendsNumber()
    {
        Assert.Equal("grass_3_3", TilesheetIds.FromFile("Grass3.png", ["grass_3", "grass_3_2"]));
    }
}
