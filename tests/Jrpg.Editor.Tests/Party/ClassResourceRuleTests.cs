using Jrpg.Core.Resources;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// A resource's four optional rules written from its boxes (decision 951): each as read, off writing nothing, typed
// rules written, the boxes' problems after the gains', each rule's typed change reaching the resource's Changed, and
// the copy a duplicated class makes of them.
public sealed class ClassResourceRuleTests
{
    private static readonly string[] _ruleNames = ["willScaling", "backfire", "strain", "dominance"];

    [Fact]
    public void From_ResourceWithBackfire_ShowsItOn()
    {
        var backfire = ClassResourceViewModel.From(Surge).Backfire;

        Assert.True(backfire.IsOn);
        string[] shown = [backfire.ChancePerPointText, backfire.DamagePercentText];
        Assert.Equal(["5", "15"], shown);
    }

    [Fact]
    public void ToResource_UntouchedResourceWithStrain_EqualsTheRecord() =>
        Assert.Equal(Toll, ClassResourceViewModel.From(Toll).ToResource());

    [Fact]
    public void ToResource_UntouchedResourceWithDominance_EqualsTheRecord() =>
        Assert.Equal(Bloom, ClassResourceViewModel.From(Bloom).ToResource());

    [Fact]
    public void ToResource_BackfireUnticked_WritesNone()
    {
        var surge = ClassResourceViewModel.From(Surge);

        surge.Backfire.IsOn = false;

        Assert.Equal(Surge with { Backfire = null }, surge.ToResource());
    }

    [Fact]
    public void ToResource_TypedRules_WritesEach()
    {
        var fury = ClassResourceViewModel.From(Fury);

        TickEveryRule(fury);
        fury.WillScaling.WillPerPointText = "6";
        fury.Backfire.DamagePercentText = "7";
        fury.Strain.LowersStartOf = "bloom";
        fury.Dominance.Over = "blight";

        Assert.Equal(
            Fury with
            {
                WillScaling = new WillScaling(6, WillEffect.RaiseMax),
                Backfire = new BackfireRule(1, 7),
                Strain = new StrainRule("bloom", 1),
                Dominance = new DominanceRule("blight", 0),
            },
            fury.ToResource());
    }

    [Theory]
    [InlineData("willScaling", "Will per point must be a whole number (was 'x').")]
    [InlineData("backfire", "Backfire chance per point must be a whole number (was 'x').")]
    [InlineData("strain", "Strain per missing point must be a whole number (was 'x').")]
    [InlineData("dominance", "Dominance bonus must be a whole number (was 'x').")]
    public void Problem_OnRuleBoxNotANumber_NamesIt(string rule, string expected)
    {
        var fury = ClassResourceViewModel.From(Fury);
        TickEveryRule(fury);

        TypeIntoNumberBox(fury, rule, "x");

        Assert.Equal(expected, fury.ProblemAmong([]));
    }

    [Fact]
    public void Problem_OffRuleBoxNotANumber_IsNull()
    {
        var fury = ClassResourceViewModel.From(Fury);

        fury.Strain.PerMissingPointText = "x";

        Assert.Null(fury.ProblemAmong([]));
    }

    // The tab's order: the gains above the rules, Will scaling the first rule.
    [Fact]
    public void Problem_GainAndRulesNotNumbers_NamesTheGainFirst()
    {
        var fury = ClassResourceViewModel.From(Fury);
        TickEveryRule(fury);
        foreach (var rule in _ruleNames)
        {
            TypeIntoNumberBox(fury, rule, "x");
        }

        fury.Gains.PerBattleFoughtText = "g";

        Assert.Equal("Per battle fought must be a whole number (was 'g').", fury.ProblemAmong([]));
    }

    [Fact]
    public void Problem_EveryRuleNotANumber_NamesWillScalingFirst()
    {
        var fury = ClassResourceViewModel.From(Fury);
        TickEveryRule(fury);

        foreach (var rule in _ruleNames)
        {
            TypeIntoNumberBox(fury, rule, "x");
        }

        Assert.Equal("Will per point must be a whole number (was 'x').", fury.ProblemAmong([]));
    }

    [Theory]
    [InlineData("willScaling")]
    [InlineData("backfire")]
    [InlineData("strain")]
    [InlineData("dominance")]
    public void Changed_RuleTyped_IsRaisedOnce(string rule)
    {
        var fury = ClassResourceViewModel.From(Fury);
        var raised = 0;
        fury.Changed += (_, _) => raised++;

        TypeIntoNumberBox(fury, rule, "9");

        Assert.Equal(1, raised);
    }

    [Fact]
    public void DuplicateOf_HalfTypedRules_CopiesThem()
    {
        var fury = ClassResourceViewModel.From(Fury);
        TickEveryRule(fury);
        fury.WillScaling.WillPerPointText = "1w";
        fury.Backfire.ChancePerPointText = "1b";
        fury.Strain.LowersStartOf = "bloom";
        fury.Dominance.BonusPercentText = "1d";

        var copy = ClassResourceViewModel.DuplicateOf(fury);

        string[] copied =
        [
            copy.WillScaling.WillPerPointText, copy.Backfire.ChancePerPointText, copy.Strain.LowersStartOf,
            copy.Dominance.BonusPercentText,
        ];
        Assert.Equal(["1w", "1b", "bloom", "1d"], copied);
        Assert.True(copy.WillScaling.IsOn && copy.Backfire.IsOn && copy.Strain.IsOn && copy.Dominance.IsOn);
    }

    private static void TickEveryRule(ClassResourceViewModel resource)
    {
        resource.WillScaling.IsOn = true;
        resource.Backfire.IsOn = true;
        resource.Strain.IsOn = true;
        resource.Dominance.IsOn = true;
    }

    private static void TypeIntoNumberBox(ClassResourceViewModel resource, string rule, string text)
    {
        switch (rule)
        {
            case "willScaling":
                resource.WillScaling.WillPerPointText = text;
                break;
            case "backfire":
                resource.Backfire.ChancePerPointText = text;
                break;
            case "strain":
                resource.Strain.PerMissingPointText = text;
                break;
            case "dominance":
                resource.Dominance.BonusPercentText = text;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rule), rule, "Not a resource rule.");
        }
    }
}
