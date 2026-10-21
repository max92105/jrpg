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
    public void FromFile_IdTaken_AppendsNumber()
    {
        Assert.Equal("grass_3_3", TilesheetIds.FromFile("Grass3.png", ["grass_3", "grass_3_2"]));
    }
}
