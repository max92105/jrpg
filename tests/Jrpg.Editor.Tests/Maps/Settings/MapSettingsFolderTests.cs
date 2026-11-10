using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Tests.Maps.Settings;

/// <summary>
/// The Map settings window's Folder box (decision 532): typed, empty for the top, copied back as typed.
/// </summary>
public sealed class MapSettingsFolderTests
{
    private const string Rule = "A folder path is names joined by '/', each with text, no space at either end and no backslash.";

    private static readonly MapDefinition _forest = new(
        new MapLayout { Id = "forest", Width = 20, Height = 15, Layers = [], Objects = [] },
        new MapSettings { Id = "forest", DisplayName = "Forest" });

    private readonly MapSettingsViewModel _settings = new(_forest, [], []);

    [Fact]
    public void FolderText_MapInAFolder_ShowsIt()
    {
        var settings = new MapSettingsViewModel(new MapDefinition(_forest.Layout, _forest.Settings with { Folder = "Act 1/Woods" }), [], []);

        Assert.Equal("Act 1/Woods", settings.FolderText);
        Assert.False(settings.AreSettingsChanged);
    }

    [Fact]
    public void ToSettings_FolderTyped_CarriesIt()
    {
        _settings.FolderText = " Act 1/Woods ";

        Assert.Equal("Act 1/Woods", _settings.ToSettings().Folder);
    }

    [Fact]
    public void ToSettings_Empty_HasNoFolder()
    {
        _settings.FolderText = " ";

        Assert.Null(_settings.ToSettings().Folder);
        Assert.Null(_settings.FolderProblem);
    }

    [Fact]
    public void AreSettingsChanged_OnlyTheFolderChanged_IsTrue()
    {
        _settings.FolderText = "Act 1";

        Assert.True(_settings.AreSettingsChanged);
    }

    [Fact]
    public void FolderProblem_BadPath_NamesTheRule()
    {
        _settings.FolderText = "Act 1//Woods";

        Assert.Equal(Rule, _settings.FolderProblem);
    }

    [Fact]
    public void Problem_OnlyTheFolderWrong_IsTheFolderProblem()
    {
        _settings.FolderText = "Act 1//Woods";

        Assert.Equal(Rule, _settings.Problem);
        Assert.False(_settings.CanApply);
    }

    [Fact]
    public void Problem_PropertyChangedOnFolderTyped_IsRaised()
    {
        var raised = new List<string?>();
        _settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _settings.FolderText = "Act 1//Woods";

        Assert.Contains(nameof(MapSettingsViewModel.Problem), raised);
        Assert.Contains(nameof(MapSettingsViewModel.CanApply), raised);
    }

    // Applying the dialog keeps every field the record gained: the copy-back rule over all three.
    [Fact]
    public void ToSettings_LevelBannerOffAndFolder_AllKept()
    {
        _settings.AreaLevelText = "5";
        _settings.ShowsBanner = false;
        _settings.FolderText = "Act 1";

        var edited = _settings.ToSettings();

        Assert.Equal((5, false, "Act 1"), (edited.AreaLevel, edited.ShowsBanner, edited.Folder));
    }
}
