using Jrpg.Editor.Encounters;
using static Jrpg.Editor.Tests.Encounters.EncounterFixtures;

namespace Jrpg.Editor.Tests.Encounters;

// The Link dialog: a map picked among those not rolling the encounter; a map without random battles has its odds typed,
// and its battle background picked when it names none.
public sealed class MapLinkViewModelTests
{
    private static MapLinkViewModel Dialog() =>
        new(
            "a_trio",
            [MapWithoutBattles("town", null), MapRolling("cave", "b_pair"), MapWithoutBattles("beach", "Forest.png")],
            ["Cave.png"]);

    private static MapLinkViewModel DialogOn(string mapId)
    {
        var dialog = Dialog();
        dialog.SelectedMap = dialog.Maps.Single(map => map.MapId == mapId);
        return dialog;
    }

    [Fact]
    public void Maps_OutOfOrder_AreListedByName()
    {
        Assert.Equal(["BEACH (beach)", "CAVE (cave)", "TOWN (town)"], Dialog().Maps.Select(map => map.Label));
    }

    [Fact]
    public void Problem_NoMapPicked_AsksForOne()
    {
        var dialog = Dialog();

        Assert.Equal("Pick a map.", dialog.Problem);
        Assert.False(dialog.CanApply);
    }

    [Fact]
    public void ToSettings_MapWithBattles_AddsTheEncounterWithoutAskingOdds()
    {
        var dialog = DialogOn("cave");

        Assert.False(dialog.AsksOdds);
        Assert.Null(dialog.Problem);
        Assert.Equal(["b_pair", "a_trio"], dialog.ToSettings().RandomEncounters!.EncounterIds);
    }

    [Fact]
    public void ToSettings_MapWithoutBattles_TurnsThemOnWithTheTypedOdds()
    {
        var dialog = DialogOn("beach");
        dialog.OneInStepsText = "30";
        dialog.GraceStepsText = "1";

        var table = dialog.ToSettings().RandomEncounters!;

        Assert.True(dialog.AsksOdds);
        Assert.Equal((30, 1), (table.OneInSteps, table.GraceSteps));
        Assert.Equal(["a_trio"], table.EncounterIds);
    }

    [Fact]
    public void ToSettings_MapWithItsOwnBackground_KeepsIt()
    {
        var dialog = DialogOn("beach");

        Assert.False(dialog.AsksBackground);
        Assert.Equal("Forest.png", dialog.ToSettings().BattleBackgroundFile);
    }

    [Fact]
    public void Problem_MapWithoutBackground_AsksForOne()
    {
        var dialog = DialogOn("town");

        Assert.True(dialog.AsksBackground);
        Assert.Equal("Random battles need a battle background.", dialog.Problem);
    }

    [Fact]
    public void ToSettings_BackgroundPicked_NamesIt()
    {
        var dialog = DialogOn("town");

        dialog.SelectedBackground = "Cave.png";

        Assert.Null(dialog.Problem);
        Assert.Equal("Cave.png", dialog.ToSettings().BattleBackgroundFile);
    }

    [Theory]
    [InlineData("0", "5")]
    [InlineData("20", "0")]
    [InlineData("", "5")]
    [InlineData("20", "five")]
    public void Problem_OddsNotAWholeNumberFromOne_NamesTheOdds(string oneInSteps, string graceSteps)
    {
        var dialog = DialogOn("beach");

        dialog.OneInStepsText = oneInSteps;
        dialog.GraceStepsText = graceSteps;

        Assert.Equal("The odds and the grace steps must be whole numbers, 1 or more.", dialog.Problem);
        Assert.Throws<InvalidOperationException>(() => dialog.ToSettings());
    }

    // A map with random battles never reads the odds boxes, so a typo left in them refuses nothing.
    [Fact]
    public void Problem_BadOddsOnAMapWithBattles_IsNull()
    {
        var dialog = DialogOn("cave");

        dialog.OneInStepsText = "";

        Assert.Null(dialog.Problem);
    }

    [Fact]
    public void SelectedMap_Changed_RaisesTheBoxesAndTheProblem()
    {
        var dialog = Dialog();
        List<string?> raised = [];
        dialog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        dialog.SelectedMap = dialog.Maps[0];

        Assert.Contains(nameof(MapLinkViewModel.AsksOdds), raised);
        Assert.Contains(nameof(MapLinkViewModel.AsksBackground), raised);
        Assert.Contains(nameof(MapLinkViewModel.CanApply), raised);
    }
}
