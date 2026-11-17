using Jrpg.Core.Resources;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// A resource's gains as typed boxes: the numbers they start on, the first box that doesn't read in the form's order,
// and the gains as typed.
public sealed class ResourceGainsBoxesTests
{
    [Fact]
    public void Constructor_Gains_ShowsEveryNumber()
    {
        var boxes = new ResourceGainsBoxes(Fury.Gains);

        Assert.Equal(["1", "2", "3", "40", "5", "6", "7", "-8"], TextsOf(boxes));
    }

    [Fact]
    public void Typed_Untouched_EqualsTheGains() => Assert.Equal(Fury.Gains, new ResourceGainsBoxes(Fury.Gains).Typed);

    [Fact]
    public void Typed_PerHitTakenTyped_TakesIt()
    {
        var boxes = new ResourceGainsBoxes(Fury.Gains);

        boxes.PerHitTakenText = " 12 ";

        Assert.Equal(Fury.Gains with { PerHitTaken = 12 }, boxes.Typed);
    }

    [Fact]
    public void Typed_BoxNotANumber_IsNull()
    {
        var boxes = new ResourceGainsBoxes(Fury.Gains);

        boxes.PerAllyFallenText = "";

        Assert.Null(boxes.Typed);
    }

    // A drain among them: the sign reads, since Data says which gains may be negative.
    [Fact]
    public void Problem_EveryBoxReads_IsNull() => Assert.Null(new ResourceGainsBoxes(Fury.Gains).Problem);

    [Fact]
    public void Problem_PerTurnNotANumber_NamesPerTurn()
    {
        var boxes = new ResourceGainsBoxes(Fury.Gains);

        boxes.PerTurnText = "two";

        Assert.Equal("Per turn must be a whole number (was 'two').", boxes.Problem);
    }

    [Fact]
    public void Problem_HeavyHitShareNotANumber_NamesTheShare()
    {
        var boxes = new ResourceGainsBoxes(Fury.Gains);

        boxes.HeavyHitPercentText = "40%";

        Assert.Equal("Heavy hit share must be a whole number (was '40%').", boxes.Problem);
    }

    // The last box: the form's order holds to the end.
    [Fact]
    public void Problem_PerBattleFoughtNotANumber_NamesPerBattleFought()
    {
        var boxes = new ResourceGainsBoxes(Fury.Gains);

        boxes.PerBattleFoughtText = "-";

        Assert.Equal("Per battle fought must be a whole number (was '-').", boxes.Problem);
    }

    // Typed last box first: the problem follows the form's order, not the order of typing.
    [Fact]
    public void Problem_TwoBoxesNotNumbers_NamesTheFirstInTheFormsOrder()
    {
        var boxes = new ResourceGainsBoxes(Fury.Gains);

        boxes.PerEnemyFallenText = "y";
        boxes.PerHitTakenText = "x";

        Assert.Equal("Per hit taken must be a whole number (was 'x').", boxes.Problem);
    }

    [Fact]
    public void CopyFrom_OtherBoxes_TakesTheirTextsAsTyped()
    {
        var copied = new ResourceGainsBoxes(Fury.Gains) { PerHitLandedText = "5x" };
        var boxes = new ResourceGainsBoxes(new ResourceGains());

        boxes.CopyFrom(copied);

        Assert.Equal(["1", "2", "3", "40", "5x", "6", "7", "-8"], TextsOf(boxes));
    }

    [Fact]
    public void PropertyChanged_BoxTyped_RaisesProblemAndTyped()
    {
        var boxes = new ResourceGainsBoxes(Fury.Gains);
        var raised = new List<string?>();
        boxes.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        boxes.PerHeavyHitText = "11";

        Assert.Contains(nameof(ResourceGainsBoxes.Problem), raised);
        Assert.Contains(nameof(ResourceGainsBoxes.Typed), raised);
        Assert.Equal(11, boxes.Typed?.PerHeavyHit);
    }
}
