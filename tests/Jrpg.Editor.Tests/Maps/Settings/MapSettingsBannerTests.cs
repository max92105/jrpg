using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Tests.Maps.Settings;

/// <summary>The Map settings window's Show banner on entry box (decision 528), copied in and back.</summary>
public sealed class MapSettingsBannerTests
{
    private static readonly MapDefinition _forest = new(
        new MapLayout { Id = "forest", Width = 20, Height = 15, Layers = [], Objects = [] },
        new MapSettings { Id = "forest", DisplayName = "Forest" });

    private readonly MapSettingsViewModel _settings = new(_forest, [], []);

    // The fixture differs from the default: a map with the banner off.
    [Fact]
    public void ShowsBanner_MapWithBannerOff_IsFalse()
    {
        var settings = new MapSettingsViewModel(new MapDefinition(_forest.Layout, _forest.Settings with { ShowsBanner = false }), [], []);

        Assert.False(settings.ShowsBanner);
        Assert.False(settings.AreSettingsChanged);
    }

    [Fact]
    public void ToSettings_BannerUnticked_CarriesFalse()
    {
        _settings.ShowsBanner = false;

        Assert.False(_settings.ToSettings().ShowsBanner);
    }

    [Fact]
    public void AreSettingsChanged_OnlyTheBannerChanged_IsTrue()
    {
        _settings.ShowsBanner = false;

        Assert.True(_settings.AreSettingsChanged);
    }

    [Fact]
    public void ToSettings_LevelAndBannerOff_BothKept()
    {
        _settings.AreaLevelText = "5";
        _settings.ShowsBanner = false;

        var edited = _settings.ToSettings();

        Assert.Equal((5, false), (edited.AreaLevel, edited.ShowsBanner));
    }
}
