using Jrpg.Core.Maps;
using Jrpg.Editor.Maps.Settings;

namespace Jrpg.Editor.Tests.Maps.Settings;

public sealed class MapSettingsViewModelTests
{
    private static readonly MapDefinition _forest = new(
        new MapLayout { Id = "forest", Width = 20, Height = 15, Layers = [], Objects = [] },
        new MapSettings
        {
            Id = "forest",
            DisplayName = "Forest",
            RandomEncounters = new RandomEncounterTable { OneInSteps = 25, GraceSteps = 3, EncounterIds = ["wolves", "goblin_trio"] },
        });

    private readonly MapSettingsViewModel _settings = new(_forest, ["wolves", "bats", "goblin_trio"]);

    [Fact]
    public void Constructor_MapWithBattles_TicksItsEncounters()
    {
        Assert.Equal(
            [("bats", false), ("goblin_trio", true), ("wolves", true)],
            _settings.Encounters.Select(choice => (choice.EncounterId, choice.IsChosen)));
    }

    [Fact]
    public void ToSettings_NoEncounters_ProducesNullTable()
    {
        _settings.HasRandomEncounters = false;

        Assert.Null(_settings.ToSettings().RandomEncounters);
    }

    [Fact]
    public void ToSettings_EncounterTicked_AddsItToTheTable()
    {
        _settings.Encounters[0].IsChosen = true;
        _settings.OneInStepsText = "40";

        var table = _settings.ToSettings().RandomEncounters!;

        Assert.Equal((40, 3), (table.OneInSteps, table.GraceSteps));
        Assert.Equal(["bats", "goblin_trio", "wolves"], table.EncounterIds);
    }

    [Fact]
    public void AreSettingsChanged_NothingEdited_IsFalse()
    {
        // The encounters come out sorted, which differs from the map's order but not in meaning.
        Assert.False(_settings.AreSettingsChanged);
        Assert.False(_settings.IsResizing);
    }

    [Fact]
    public void AreSettingsChanged_NameEdited_IsTrue()
    {
        _settings.DisplayName = "Dark forest";

        Assert.True(_settings.AreSettingsChanged);
    }

    [Fact]
    public void IsShrinking_NarrowerMap_IsTrue()
    {
        _settings.WidthText = "19";
        _settings.HeightText = "30";

        Assert.True(_settings.IsShrinking);
    }

    [Fact]
    public void SizeProblem_ZeroHeight_SaysTheLimits()
    {
        _settings.HeightText = "0";

        Assert.Equal($"Width and height must be whole numbers from 1 to {MapSettingsViewModel.MaxSize}.", _settings.SizeProblem);
        Assert.Null(_settings.NewSize);
    }

    [Fact]
    public void NewSize_BoxClearedAfterTyping_IsNull()
    {
        // Backspacing "20" passes through "2"; the empty box must not keep it.
        _settings.WidthText = "2";

        _settings.WidthText = "";

        Assert.Null(_settings.NewSize);
        Assert.False(_settings.IsResizing);
    }

    [Fact]
    public void OddsProblem_GraceStepsNotANumber_SaysSo()
    {
        _settings.GraceStepsText = "three";

        Assert.NotNull(_settings.OddsProblem);
    }

    [Fact]
    public void OddsProblem_NoRandomBattles_IsNull()
    {
        _settings.GraceStepsText = "";

        _settings.HasRandomEncounters = false;

        Assert.Null(_settings.OddsProblem);
    }
}
