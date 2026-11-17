using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// A class's base or growth as eight typed boxes: the numbers it starts on, the first box that doesn't read in the
// page's order, and the stats as typed.
public sealed class ClassStatsBoxesTests
{
    [Fact]
    public void Constructor_Stats_ShowsEveryNumber()
    {
        var boxes = new ClassStatsBoxes(Scholar.Base, "Base");

        Assert.Equal(["140", "9", "3", "11", "7", "17", "5", "13"], TextsOf(boxes));
    }

    [Fact]
    public void Typed_Untouched_EqualsTheStats()
    {
        var boxes = new ClassStatsBoxes(Scholar.Growth, "Growth");

        Assert.Equal(Scholar.Growth, boxes.Typed);
    }

    [Fact]
    public void Typed_PowerTyped_TakesIt()
    {
        var boxes = new ClassStatsBoxes(Scholar.Growth, "Growth");

        boxes.PowerText = " 7 ";

        Assert.Equal(Scholar.Growth with { Primaries = Scholar.Growth.Primaries with { Power = 7 } }, boxes.Typed);
    }

    [Fact]
    public void Typed_BoxNotANumber_IsNull()
    {
        var boxes = new ClassStatsBoxes(Scholar.Base, "Base");

        boxes.AgilityText = "";

        Assert.Null(boxes.Typed);
    }

    [Fact]
    public void Problem_EveryBoxReads_IsNull() => Assert.Null(new ClassStatsBoxes(Scholar.Base, "Base").Problem);

    [Fact]
    public void Problem_MaxHpNotANumber_NamesItAfterTheLabel()
    {
        var boxes = new ClassStatsBoxes(Scholar.Base, "Base");

        boxes.MaxHpText = "ten";

        Assert.Equal("Base max HP must be a whole number (was 'ten').", boxes.Problem);
    }

    // The last box: the page's order holds to the end.
    [Fact]
    public void Problem_WillNotANumber_NamesWill()
    {
        var boxes = new ClassStatsBoxes(Scholar.Growth, "Growth");

        boxes.WillText = "x";

        Assert.Equal("Growth will must be a whole number (was 'x').", boxes.Problem);
    }

    // Typed last box first: the problem follows the page's order, not the order of typing.
    [Fact]
    public void Problem_TwoBoxesNotNumbers_NamesTheFirstInThePagesOrder()
    {
        var boxes = new ClassStatsBoxes(Scholar.Base, "Base");

        boxes.ConstitutionText = "y";
        boxes.SpeedText = "x";

        Assert.Equal("Base speed must be a whole number (was 'x').", boxes.Problem);
    }

    [Fact]
    public void CopyFrom_OtherBoxes_TakesTheirTextsAsTyped()
    {
        var copied = new ClassStatsBoxes(Scholar.Growth, "Growth") { AccuracyText = "4x" };
        var boxes = new ClassStatsBoxes(new(MaxHp: 1, Speed: 0), "Growth");

        boxes.CopyFrom(copied);

        Assert.Equal(["12", "2", "4x", "6", "8", "10", "14", "16"], TextsOf(boxes));
    }

    [Fact]
    public void PropertyChanged_BoxTyped_RaisesProblemAndTyped()
    {
        var boxes = new ClassStatsBoxes(Scholar.Base, "Base");
        var raised = new List<string?>();
        boxes.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        boxes.IntelligenceText = "18";

        Assert.Contains(nameof(ClassStatsBoxes.Problem), raised);
        Assert.Contains(nameof(ClassStatsBoxes.Typed), raised);
        Assert.Equal(18, boxes.Typed?.Primaries.Intelligence);
    }
}
