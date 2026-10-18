using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Tests.Maps.Settings;

public sealed class NewMapViewModelTests
{
    private readonly NewMapViewModel _newMap = new(["forest", "start"]);

    [Fact]
    public void MapId_NameTyped_FollowsTheName()
    {
        _newMap.Name = "Forest";

        Assert.Equal("forest_2", _newMap.MapId);
    }

    [Fact]
    public void ToMap_ValidFields_MakesAnEmptyMapWithoutBattles()
    {
        _newMap.Name = " Old Mill ";
        _newMap.WidthText = "12";
        _newMap.HeightText = "8";

        var map = _newMap.ToMap();

        Assert.Equal(("old_mill", "old_mill", "Old Mill"), (map.Layout.Id, map.Settings.Id, map.Settings.DisplayName));
        Assert.Equal((12, 8), (map.Layout.Width, map.Layout.Height));
        Assert.Empty(map.Layout.Layers);
        Assert.Empty(map.Layout.Objects);
        Assert.Null(map.Settings.RandomEncounters);
    }

    [Fact]
    public void NameProblem_BlankName_SaysSo()
    {
        _newMap.Name = "  ";

        Assert.NotNull(_newMap.NameProblem);
    }

    [Fact]
    public void SizeProblem_EmptyWidth_SaysTheLimits()
    {
        _newMap.WidthText = "";

        Assert.Equal(TypedCounts.MapSizeProblem, _newMap.SizeProblem);
    }
}
