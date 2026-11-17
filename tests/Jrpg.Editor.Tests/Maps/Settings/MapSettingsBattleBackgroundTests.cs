using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Tests.Maps.Settings;

/// <summary>
/// The Map settings window's Battle background box (decisions 892, 909): "(none)" first, then the backgrounds folder's
/// images; a map with random battles needs one.
/// </summary>
public sealed class MapSettingsBattleBackgroundTests
{
    private const string BackgroundProblem = "Random battles need a battle background.";

    private static readonly IReadOnlyList<string> _folder = ["Cave.png", "Forest.png"];

    private static readonly MapDefinition _forest = new(
        new MapLayout { Id = "forest", Width = 20, Height = 15, Layers = [], Objects = [] },
        new MapSettings
        {
            Id = "forest",
            DisplayName = "Forest",
            BattleBackgroundFile = "Forest.png",
            RandomEncounters = new RandomEncounterTable { OneInSteps = 25, GraceSteps = 3, EncounterIds = ["wolves"] },
        });

    private readonly MapSettingsViewModel _settings = new(_forest, ["wolves"], [], _folder);

    [Fact]
    public void Constructor_MapNamingABackground_SelectsIt()
    {
        Assert.Equal("Forest.png", _settings.SelectedBattleBackground);
        Assert.Equal([MapSettingsViewModel.NoBattleBackground, "Cave.png", "Forest.png"], _settings.BattleBackgroundChoices);
        Assert.False(_settings.AreSettingsChanged);
    }

    [Fact]
    public void Constructor_MapWithoutABackground_SelectsNone()
    {
        var settings = Opened(_forest.Settings with { BattleBackgroundFile = null, RandomEncounters = null });

        Assert.Equal(MapSettingsViewModel.NoBattleBackground, settings.SelectedBattleBackground);
    }

    [Fact]
    public void Constructor_BackgroundNoLongerInTheFolder_StaysListed()
    {
        var settings = Opened(_forest.Settings with { BattleBackgroundFile = "Gone.png" });

        Assert.Equal(
            [MapSettingsViewModel.NoBattleBackground, "Gone.png", "Cave.png", "Forest.png"], settings.BattleBackgroundChoices);
        Assert.Equal("Gone.png", settings.ToSettings().BattleBackgroundFile);
        Assert.False(settings.AreSettingsChanged);
    }

    [Fact]
    public void ToSettings_NoneChosen_WritesNull()
    {
        _settings.SelectedBattleBackground = MapSettingsViewModel.NoBattleBackground;

        Assert.Null(_settings.ToSettings().BattleBackgroundFile);
    }

    // The choice differs from the map's, so a copy of the map's own background fails.
    [Fact]
    public void ToSettings_ImageChosen_WritesIt()
    {
        _settings.SelectedBattleBackground = "Cave.png";

        Assert.Equal("Cave.png", _settings.ToSettings().BattleBackgroundFile);
    }

    [Fact]
    public void Problem_RandomBattlesAndNone_NamesTheBackground()
    {
        _settings.SelectedBattleBackground = MapSettingsViewModel.NoBattleBackground;

        Assert.Equal(BackgroundProblem, _settings.BattleBackgroundProblem);
        Assert.Equal(BackgroundProblem, _settings.Problem);
        Assert.False(_settings.CanApply);
    }

    [Fact]
    public void Problem_RandomBattlesOffAndNone_IsNull()
    {
        _settings.SelectedBattleBackground = MapSettingsViewModel.NoBattleBackground;

        _settings.HasRandomEncounters = false;

        Assert.Null(_settings.Problem);
        Assert.True(_settings.CanApply);
    }

    [Fact]
    public void Problem_RandomBattlesAndAnImage_IsNull()
    {
        _settings.SelectedBattleBackground = "Cave.png";

        Assert.Null(_settings.Problem);
        Assert.True(_settings.CanApply);
    }

    [Fact]
    public void Problem_OddsAndBackgroundWrong_IsTheOddsProblem()
    {
        _settings.SelectedBattleBackground = MapSettingsViewModel.NoBattleBackground;
        _settings.OneInStepsText = "0";

        Assert.Equal(_settings.OddsProblem, _settings.Problem);
        Assert.NotNull(_settings.OddsProblem);
    }

    [Fact]
    public void HasRandomEncounters_TurnedOn_RaisesProblemChanged()
    {
        var settings = Opened(_forest.Settings with { BattleBackgroundFile = null, RandomEncounters = null });
        var raised = new List<string?>();
        settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        settings.HasRandomEncounters = true;

        Assert.Contains(nameof(MapSettingsViewModel.BattleBackgroundProblem), raised);
        Assert.Contains(nameof(MapSettingsViewModel.Problem), raised);
        Assert.Contains(nameof(MapSettingsViewModel.CanApply), raised);
        Assert.Equal(BackgroundProblem, settings.Problem);
    }

    [Fact]
    public void SelectedBattleBackground_NoneChosen_RaisesProblemChanged()
    {
        var raised = new List<string?>();
        _settings.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _settings.SelectedBattleBackground = MapSettingsViewModel.NoBattleBackground;

        Assert.Contains(nameof(MapSettingsViewModel.BattleBackgroundProblem), raised);
        Assert.Contains(nameof(MapSettingsViewModel.Problem), raised);
        Assert.Contains(nameof(MapSettingsViewModel.CanApply), raised);
    }

    [Fact]
    public void AreSettingsChanged_OnlyTheBackgroundChanged_IsTrue()
    {
        _settings.SelectedBattleBackground = "Cave.png";

        Assert.True(_settings.AreSettingsChanged);
    }

    private static MapSettingsViewModel Opened(MapSettings settings) =>
        new(new MapDefinition(_forest.Layout, settings), ["wolves"], [], _folder);
}
