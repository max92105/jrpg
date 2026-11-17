using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Tests.Maps.Settings;

/// <summary>
/// The Map settings window keeps the map's battle background (decision 892): the dialog rebuilds its settings, so a
/// field it has no box for yet is copied from the map.
/// </summary>
public sealed class MapSettingsBattleBackgroundTests
{
    [Fact]
    public void ToSettings_MapWithABackground_KeepsIt()
    {
        var forest = new MapDefinition(
            new MapLayout { Id = "forest", Width = 20, Height = 15, Layers = [], Objects = [] },
            new MapSettings { Id = "forest", DisplayName = "Forest", BattleBackgroundFile = "Arena.png" });
        var settings = new MapSettingsViewModel(forest, [], []);
        settings.DisplayName = "Deep forest";

        var edited = settings.ToSettings();

        Assert.Equal("Arena.png", edited.BattleBackgroundFile);
    }
}
