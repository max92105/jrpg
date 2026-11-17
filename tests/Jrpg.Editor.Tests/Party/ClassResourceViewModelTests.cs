using Jrpg.Core.Resources;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// One resource as the Resources tab types it, written over the record it came from: its boxes, its amounts fixed or
// from speed, its problem, its label, the Changed event the class's list listens to, and the copy a duplicated class
// makes of it.
public sealed class ClassResourceViewModelTests
{
    [Fact]
    public void From_Resource_ShowsEveryGain() =>
        Assert.Equal(["1", "2", "3", "40", "5", "6", "7", "-8"], TextsOf(ClassResourceViewModel.From(Fury).Gains));

    [Fact]
    public void From_Resource_ShowsItsFields()
    {
        var fury = ClassResourceViewModel.From(Fury with { IsLasting = true });

        string[] shown = [fury.Id, fury.Name, fury.ColorText, fury.MaxText, fury.StartsAtText];
        Assert.Equal(["fury", "Fury", "#C83232", "9", "1"], shown);
        Assert.True(fury.IsLasting);
        Assert.False(fury.TakesMaxFromSpeed);
    }

    [Fact]
    public void From_SpeedResource_TicksFromSpeed()
    {
        var surge = ClassResourceViewModel.From(Surge);

        Assert.True(surge.TakesMaxFromSpeed);
        Assert.Equal("10", surge.SpeedPerChargeText);
    }

    [Fact]
    public void New_IdAndNameTyped_IsTheBlankResource()
    {
        var added = ClassResourceViewModel.New();

        added.Id = "focus";
        added.Name = "Focus";

        Assert.True(added.IsNew);
        Assert.Equal(new ResourceDefinition("focus", "Focus", Max: 1, StartsAt: 0) { Color = "#FFFFFF" }, added.ToResource());
    }

    [Fact]
    public void ToResource_UntouchedResource_EqualsTheRecord() =>
        Assert.Equal(Fury, ClassResourceViewModel.From(Fury).ToResource());

    // Backfire and Will scaling come back from their boxes as the record has them.
    [Fact]
    public void ToResource_UntouchedResource_KeepsItsRules() =>
        Assert.Equal(Surge, ClassResourceViewModel.From(Surge).ToResource());

    [Fact]
    public void ToResource_TypedPerHitTaken_SetsIt()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.Gains.PerHitTakenText = "12";

        Assert.Equal(Fury with { Gains = Fury.Gains with { PerHitTaken = 12 } }, fury.ToResource());
    }

    [Fact]
    public void ToResource_TypedFields_WritesThemTrimmed()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.Name = " Rage ";
        fury.ColorText = " #102030 ";
        fury.IsLasting = true;
        fury.StartsAtText = "4";

        Assert.Equal(Fury with { Name = "Rage", Color = "#102030", IsLasting = true, StartsAt = 4 }, fury.ToResource());
    }

    // Max typed before ticking: Data asks for 0 beside a speed scaling.
    [Fact]
    public void ToResource_FromSpeed_ClearsMaxAndStart()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.MaxText = "5";
        fury.TakesMaxFromSpeed = true;

        Assert.Equal(Fury with { Max = 0, StartsAt = 0, MaxFromSpeed = new SpeedScaling(1) }, fury.ToResource());
    }

    [Fact]
    public void ToResource_FixedAmounts_ClearsSpeedScaling()
    {
        var surge = ClassResourceViewModel.From(Surge);

        surge.TakesMaxFromSpeed = false;
        surge.MaxText = "3";
        surge.StartsAtText = "2";

        Assert.Equal(Surge with { Max = 3, StartsAt = 2, MaxFromSpeed = null }, surge.ToResource());
    }

    [Fact]
    public void ToResource_BoxNotANumber_Throws()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.StartsAtText = "";

        Assert.Throws<InvalidOperationException>(() => fury.ToResource());
    }

    [Fact]
    public void Problem_SavedResourceUntouched_IsNull() => Assert.Null(ClassResourceViewModel.From(Fury).ProblemAmong([]));

    [Fact]
    public void Problem_FixedAmounts_NamesMaxBox()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.MaxText = "x";

        Assert.Equal("Max must be a whole number (was 'x').", fury.ProblemAmong([]));
    }

    [Fact]
    public void Problem_FromSpeed_IgnoresMaxBox()
    {
        var surge = ClassResourceViewModel.From(Surge);

        surge.MaxText = "x";

        Assert.Null(surge.ProblemAmong([]));
    }

    [Fact]
    public void Problem_FromSpeed_IgnoresStartsAtBox()
    {
        var surge = ClassResourceViewModel.From(Surge);

        surge.StartsAtText = "x";

        Assert.Null(surge.ProblemAmong([]));
    }

    [Fact]
    public void Problem_FromSpeed_NamesSpeedPerChargeBox()
    {
        var surge = ClassResourceViewModel.From(Surge);

        surge.SpeedPerChargeText = "ten";

        Assert.Equal("Speed per charge must be a whole number (was 'ten').", surge.ProblemAmong([]));
    }

    [Fact]
    public void Problem_FixedAmounts_IgnoresSpeedPerChargeBox()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.SpeedPerChargeText = "ten";

        Assert.Null(fury.ProblemAmong([]));
    }

    [Fact]
    public void Problem_StartsAtNotANumber_NamesIt()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.StartsAtText = "1.5";

        Assert.Equal("Starts at must be a whole number (was '1.5').", fury.ProblemAmong([]));
    }

    [Fact]
    public void Problem_GainNotANumber_NamesTheGain()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.Gains.PerEnemyFallenText = "?";

        Assert.Equal("Per enemy fallen must be a whole number (was '?').", fury.ProblemAmong([]));
    }

    [Fact]
    public void Problem_NewResourceWithClassmateId_NamesTheId()
    {
        var added = ClassResourceViewModel.New();

        added.Id = "fury";

        Assert.Equal("The id fury is taken.", added.ProblemAmong(["fury"]));
    }

    [Fact]
    public void Problem_NewResourceWithFreeId_IsNull()
    {
        var added = ClassResourceViewModel.New();

        added.Id = "focus";
        added.Name = "Focus";

        Assert.Null(added.ProblemAmong(["fury"]));
    }

    [Fact]
    public void Problem_SavedResource_IgnoresIdRule() =>
        Assert.Null(ClassResourceViewModel.From(Fury).ProblemAmong(["fury"]));

    [Fact]
    public void Label_SavedResource_ShowsIdAndName() => Assert.Equal("fury: Fury", ClassResourceViewModel.From(Fury).Label);

    [Fact]
    public void Label_NewResourceWithoutId_SaysNewResource() =>
        Assert.Equal("(new resource)", ClassResourceViewModel.New().Label);

    // Each typed field once, the label's notices never: eight raises.
    [Fact]
    public void Changed_EachFieldTyped_IsRaisedOncePerField()
    {
        var added = ClassResourceViewModel.New();
        var raised = 0;
        added.Changed += (_, _) => raised++;

        added.Id = "focus";
        added.Name = "Focus";
        added.ColorText = "#000000";
        added.IsLasting = true;
        added.TakesMaxFromSpeed = true;
        added.MaxText = "2";
        added.StartsAtText = "1";
        added.SpeedPerChargeText = "8";

        Assert.Equal(8, raised);
    }

    [Fact]
    public void Changed_GainTyped_IsRaised()
    {
        var fury = ClassResourceViewModel.From(Fury);
        var raised = 0;
        fury.Changed += (_, _) => raised++;

        fury.Gains.PerTurnText = "3";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void DuplicateOf_HalfTypedBox_CopiesTheTextsAndId()
    {
        var fury = ClassResourceViewModel.From(Fury);
        fury.MaxText = "9x";
        fury.Gains.PerTurnText = "2y";

        var copy = ClassResourceViewModel.DuplicateOf(fury);

        Assert.True(copy.IsNew);
        Assert.Equal(["fury", "9x", "2y"], [copy.Id, copy.MaxText, copy.Gains.PerTurnText]);
    }

    // Every field typed away from the record, so a field the copy reads from the record instead shows; the fixed
    // amounts in the next test, as From speed sets them aside.
    [Fact]
    public void DuplicateOf_TypedSpeedResource_WritesTheSameRecord()
    {
        var fury = ClassResourceViewModel.From(Fury);
        fury.Name = "Rage";
        fury.ColorText = "#010203";
        fury.TakesMaxFromSpeed = true;
        fury.SpeedPerChargeText = "7";
        fury.Gains.PerHitLandedText = "11";

        var copy = ClassResourceViewModel.DuplicateOf(fury);

        Assert.Equal(fury.ToResource(), copy.ToResource());
    }

    [Fact]
    public void DuplicateOf_TypedFixedAmounts_WritesTheSameRecord()
    {
        var fury = ClassResourceViewModel.From(Fury);
        fury.IsLasting = true;
        fury.MaxText = "12";
        fury.StartsAtText = "3";

        var copy = ClassResourceViewModel.DuplicateOf(fury);

        Assert.Equal(fury.ToResource(), copy.ToResource());
    }
}
