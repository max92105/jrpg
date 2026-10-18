using Jrpg.Editor.Maps.Editing;

namespace Jrpg.Editor.Tests.Maps.Editing;

public sealed class MapIdsTests
{
    [Fact]
    public void FromName_TakenId_AppendsNumber()
    {
        Assert.Equal("forest_3", MapIds.FromName("Forest", ["forest", "forest_2", "start"]));
    }

    [Fact]
    public void FromName_SpacesAndSymbols_MakesSnakeCase()
    {
        Assert.Equal("old_mill_east_2", MapIds.FromName("  Old Mill -- East 2! ", []));
    }

    [Fact]
    public void FromName_TakenWithOtherCase_CountsAsTaken()
    {
        // Map ids are file names, which Windows doesn't tell apart by case.
        Assert.Equal("forest_2", MapIds.FromName("forest", ["Forest"]));
    }

    [Fact]
    public void FromName_NoLettersOrDigits_UsesMap()
    {
        Assert.Equal("map", MapIds.FromName("!?", []));
    }
}
