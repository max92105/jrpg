using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Editor.Party;
using static Jrpg.Editor.Tests.Party.ClassFixtures;

namespace Jrpg.Editor.Tests.Party;

// A class's resources as typed (decision 950): listed in the record's order, written back by ToClass, their own
// problems on the class's line, Data's rules on the list's, and copied as typed with the class.
public sealed class ClassResourceListTests
{
    // Surge before Fury, opposite to id order, so a list sorted by id shows.
    private static readonly ClassDefinition _scholar = Scholar with { Resources = [Surge, Fury] };

    private static readonly ContentCatalog _draft =
        World with { Classes = new Dictionary<string, ClassDefinition> { [_scholar.Id] = _scholar } };

    // Every rule: Will scaling and backfire on Surge, Bloom and Blight naming each other, Toll straining Bloom.
    private static readonly ClassDefinition _ruledScholar = Scholar with { Resources = [Surge, Bloom, Blight, Toll] };

    private static readonly ContentCatalog _ruledDraft =
        World with { Classes = new Dictionary<string, ClassDefinition> { [_ruledScholar.Id] = _ruledScholar } };

    private readonly ClassViewModel _class = ClassViewModel.From(_scholar, _draft);

    [Fact]
    public void Resources_FromClass_ListsThemInItsOrder() =>
        Assert.Equal(["surge", "fury"], _class.Resources.Select(resource => resource.Id));

    [Fact]
    public void ToClass_UntouchedClass_EqualsTheRecord() => Assert.Equal(_scholar, _class.ToClass());

    [Fact]
    public void ToClass_TypedResourceName_WritesIt()
    {
        _class.Resources[1].Name = "Rage";

        Assert.Equal([Surge, Fury with { Name = "Rage" }], _class.ToClass().Resources);
    }

    [Fact]
    public void Problem_ResourceBoxNotANumber_NamesTheResource()
    {
        _class.Resources[1].MaxText = "x";

        Assert.Equal("Resource fury: Max must be a whole number (was 'x').", _class.Problem);
    }

    // The resources come last, as their tab does.
    [Fact]
    public void Problem_GrowthAndResourceBoxNotNumbers_NamesTheGrowthFirst()
    {
        _class.Resources[0].SpeedPerChargeText = "s";
        _class.Growth.WillText = "w";

        Assert.Equal("Growth will must be a whole number (was 'w').", _class.Problem);
    }

    [Fact]
    public void Problem_StartAboveMax_ShowsDataMessage() =>
        Assert.Equal(
            "classes.json: class 'scholar': resource 'fury': startsAt must be within 0..9 (was 10).",
            CatalogProblemWithFuryStartingAt("10"));

    [Fact]
    public void Problem_StartEqualToMax_IsNull() => Assert.Null(CatalogProblemWithFuryStartingAt("9"));

    [Fact]
    public void Problem_StartBelowZero_ShowsDataMessage() =>
        Assert.Equal(
            "classes.json: class 'scholar': resource 'fury': startsAt must be within 0..9 (was -1).",
            CatalogProblemWithFuryStartingAt("-1"));

    [Fact]
    public void Problem_StartZero_IsNull() => Assert.Null(CatalogProblemWithFuryStartingAt("0"));

    [Fact]
    public void Problem_ResourceBoxTyped_RaisesProblemChanged()
    {
        var raised = new List<string?>();
        _class.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _class.Resources[0].Gains.PerTurnText = "4";

        Assert.Contains(nameof(ClassViewModel.Problem), raised);
    }

    [Fact]
    public void Changed_ResourceTyped_IsRaisedOnce()
    {
        var raised = 0;
        _class.Changed += (_, _) => raised++;

        _class.Resources[1].ColorText = "#000000";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void DuplicateOf_HalfTypedResource_CopiesItAsNew()
    {
        _class.Resources[1].Gains.PerTurnText = "1x";

        var copy = ClassViewModel.DuplicateOf(_class);

        Assert.Equal(["surge", "fury"], copy.Resources.Select(resource => resource.Id));
        Assert.Equal("1x", copy.Resources[1].Gains.PerTurnText);
        Assert.All(copy.Resources, resource => Assert.True(resource.IsNew));
    }

    // The copy's resources are new, so their ids can be typed and are checked against each other.
    [Fact]
    public void Problem_CopiedResourceGivenClassmateId_NamesTheId()
    {
        var copy = ClassViewModel.DuplicateOf(_class);
        copy.Id = "sage";

        copy.Resources[0].Id = "fury";

        Assert.Equal("Resource fury: The id fury is taken.", copy.Problem);
    }

    [Fact]
    public void Changed_CopiedResourceTyped_IsRaised()
    {
        var copy = ClassViewModel.DuplicateOf(_class);
        var raised = 0;
        copy.Changed += (_, _) => raised++;

        copy.Resources[0].Name = "Rush";

        Assert.Equal(1, raised);
    }

    // The page's round trip: every rule through its boxes and back, the class equal to its record by content.
    [Fact]
    public void ToClass_UntouchedClassWithEveryRule_EqualsTheRecord() =>
        Assert.Equal(_ruledScholar, ClassViewModel.From(_ruledScholar, _ruledDraft).ToClass());

    [Fact]
    public void Problem_EveryRuleAsRead_IsNull() => Assert.Null(CatalogProblemWithRuledClass(_ => { }));

    [Fact]
    public void Problem_StrainOnNonLastingResource_ShowsDataMessage() =>
        Assert.Equal(
            "classes.json: class 'scholar': resource 'toll': strain is only for a lasting resource.",
            CatalogProblemWithRuledClass(resources => resources[3].IsLasting = false));

    [Fact]
    public void Problem_StrainOntoLastingResource_ShowsDataMessage() =>
        Assert.Equal(
            "classes.json: class 'scholar': resource 'toll': strain.lowersStartOf can't name a lasting resource ('bloom'), "
            + "whose lowered start would carry on and drop again.",
            CatalogProblemWithRuledClass(resources => resources[1].IsLasting = true));

    [Fact]
    public void Problem_DominanceOneSided_ShowsDataMessage() =>
        Assert.Equal(
            "classes.json: class 'scholar': resource 'bloom': dominance.over names 'blight', whose dominance doesn't name "
            + "'bloom' back.",
            CatalogProblemWithRuledClass(resources => resources[2].Dominance.IsOn = false));

    private static string? CatalogProblemWithFuryStartingAt(string startsAt)
    {
        var catalog = new ClassCatalogViewModel(_draft);
        catalog.SelectedClass!.Resources[1].StartsAtText = startsAt;
        return catalog.Problem;
    }

    private static string? CatalogProblemWithRuledClass(Action<IList<ClassResourceViewModel>> type)
    {
        var catalog = new ClassCatalogViewModel(_ruledDraft);
        type(catalog.SelectedClass!.Resources);
        return catalog.Problem;
    }
}
