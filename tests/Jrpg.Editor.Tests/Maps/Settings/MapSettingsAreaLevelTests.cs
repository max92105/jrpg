using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Tests.Maps.Settings;

/// <summary>The Map settings window's area level (decision 527): typed, empty for none, copied back as typed.</summary>
public sealed class MapSettingsAreaLevelTests
{
    private const string LevelProblem = "The area level is empty or a whole number of at least 1.";

    private static readonly MapDefinition _forest = new(
        new MapLayout { Id = "forest", Width = 20, Height = 15, Layers = [], Objects = [] },
        new MapSettings { Id = "forest", DisplayName = "Forest" });

    private readonly MapSettingsViewModel _settings = new(_forest, [], []);

    [Fact]
    public void AreaLevelText_MapWithLevel_ShowsIt()
    {
        var settings = new MapSettingsViewModel(new MapDefinition(_forest.Layout, _forest.Settings with { AreaLevel = 7 }), [], []);

        Assert.Equal("7", settings.AreaLevelText);
        Assert.False(settings.AreSettingsChanged);
    }

    [Fact]
    public void ToSettings_LevelTyped_CarriesIt()
    {
        _settings.AreaLevelText = " 12 ";

        Assert.Equal(12, _settings.ToSettings().AreaLevel);
    }

    [Fact]
    public void ToSettings_Empty_HasNoLevel()
    {
        _settings.AreaLevelText = "";

        Assert.Null(_settings.ToSettings().AreaLevel);
    }

    [Fact]
    public void AreSettingsChanged_OnlyTheLevelChanged_IsTrue()
    {
        _settings.AreaLevelText = "3";

        Assert.True(_settings.AreSettingsChanged);
    }

    [Fact]
    public void AreaLevelProblem_Zero_NamesTheRule()
    {
        _settings.AreaLevelText = "0";

        Assert.Equal(LevelProblem, _settings.AreaLevelProblem);
    }

    [Fact]
    public void AreaLevelProblem_One_IsNull()
    {
        _settings.AreaLevelText = "1";

        Assert.Null(_settings.AreaLevelProblem);
    }

    [Fact]
    public void AreaLevelProblem_NotANumber_NamesTheRule()
    {
        _settings.AreaLevelText = "five";

        Assert.Equal(LevelProblem, _settings.AreaLevelProblem);
    }

    [Fact]
    public void Problem_OnlyTheLevelWrong_IsTheLevelProblem()
    {
        _settings.AreaLevelText = "0";

        Assert.Equal(LevelProblem, _settings.Problem);
        Assert.False(_settings.CanApply);
    }

    [Fact]
    public void Problem_PropertyChangedOnLevelTyped_IsRaised()
    {
        var raised = new List<string?>();
        _settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _settings.AreaLevelText = "0";

        Assert.Contains(nameof(MapSettingsViewModel.Problem), raised);
        Assert.Contains(nameof(MapSettingsViewModel.CanApply), raised);
    }

    [Fact]
    public void Problem_SizeWrong_IsTheSizeProblem()
    {
        _settings.WidthText = "0";

        Assert.Equal(TypedCounts.MapSizeProblem, _settings.Problem);
    }

    // Every field feeding Problem tells the window to recheck OK; the level's is tested above.
    [Fact]
    public void Problem_PropertyChangedOnSizeTyped_IsRaised()
    {
        var raised = new List<string?>();
        _settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _settings.WidthText = "0";
        _settings.HeightText = "0";

        Assert.Equal(2, raised.Count(name => name == nameof(MapSettingsViewModel.Problem)));
        Assert.Equal(2, raised.Count(name => name == nameof(MapSettingsViewModel.CanApply)));
    }

    [Fact]
    public void Problem_PropertyChangedOnOddsTyped_IsRaised()
    {
        var raised = new List<string?>();
        _settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _settings.HasRandomEncounters = true;
        _settings.OneInStepsText = "x";
        _settings.GraceStepsText = "x";

        Assert.Equal(3, raised.Count(name => name == nameof(MapSettingsViewModel.Problem)));
        Assert.Equal(3, raised.Count(name => name == nameof(MapSettingsViewModel.CanApply)));
    }
}
