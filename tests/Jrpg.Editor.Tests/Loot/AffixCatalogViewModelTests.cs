using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Editor.Loot;

namespace Jrpg.Editor.Tests.Loot;

// The Affixes page's list (decision 884): the pool in id order, the added affixes after, only an added one removable,
// and the problem line ending with Data's message for the pool.
public sealed class AffixCatalogViewModelTests
{
    [Fact]
    public void Affixes_Constructed_InIdOrder()
    {
        var pool = new Dictionary<string, AffixDefinition>(StringComparer.Ordinal)
        {
            [AffixFixtures.Fireproof.Id] = AffixFixtures.Fireproof,
            [AffixFixtures.Fierce.Id] = AffixFixtures.Fierce,
        };

        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog with { Affixes = pool });

        Assert.Equal(["fierce", "fireproof"], catalog.Affixes.Select(affix => affix.Id));
    }

    [Fact]
    public void Problem_Constructed_IsNull()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void AddAffix_AddsANewOneLastAndChoosesIt()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);

        catalog.AddAffixCommand.Execute(null);

        var added = catalog.Affixes[^1];
        Assert.Same(added, catalog.SelectedAffix);
        Assert.Equal((true, ""), (added.IsNew, added.Id));
    }

    [Fact]
    public void Duplicate_ChosenAffix_AddsACopyLast()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        catalog.SelectedAffix = AffixOf(catalog, "fierce");

        catalog.DuplicateCommand.Execute(null);

        var copy = catalog.Affixes[^1];
        Assert.Same(copy, catalog.SelectedAffix);
        copy.Id = "fierce_2";
        Assert.Equal(AffixFixtures.Fierce with { Id = "fierce_2" }, copy.ToAffix());
    }

    [Fact]
    public void Duplicate_NothingChosen_CannotExecute()
    {
        var noAffixes = AffixFixtures.Catalog with { Affixes = new Dictionary<string, AffixDefinition>() };
        var catalog = new AffixCatalogViewModel(noAffixes);

        Assert.False(catalog.DuplicateCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveAffix_SavedAffixChosen_CannotExecute()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        catalog.SelectedAffix = AffixOf(catalog, "fierce");

        Assert.False(catalog.RemoveAffixCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveAffix_NewAffix_RemovesIt()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        catalog.AddAffixCommand.Execute(null);
        var removed = catalog.SelectedAffix!;

        catalog.RemoveAffixCommand.Execute(null);

        Assert.DoesNotContain(removed, catalog.Affixes);
        Assert.Same(catalog.Affixes[^1], catalog.SelectedAffix);
    }

    [Fact]
    public void SelectedAffix_Changed_RaisesTheListButtonsCanExecuteChanged()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        var raised = new List<string>();
        catalog.DuplicateCommand.CanExecuteChanged += (_, _) => raised.Add("Duplicate");
        catalog.RemoveAffixCommand.CanExecuteChanged += (_, _) => raised.Add("Remove");

        catalog.SelectedAffix = AffixOf(catalog, "fireproof");

        Assert.Equal(["Duplicate", "Remove"], raised);
    }

    [Fact]
    public void Problem_AffixWithNoKind_IsDatasMessage()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        catalog.SelectedAffix = AffixOf(catalog, "fireproof");

        AffixFixtures.Toggle(catalog.SelectedAffix, EquipmentKind.Chest).IsChosen = false;

        Assert.Equal("affixes.json: affix 'fireproof': kinds must list at least one kind of gear.", catalog.Problem);
    }

    // The fierce blade fixes fierce at 5: free of the range, not of its side of zero (decision 866).
    [Fact]
    public void Problem_RangeMovedBelowZeroUnderAUnique_IsTheItemsMessage()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        catalog.SelectedAffix = AffixOf(catalog, "fierce");

        catalog.SelectedAffix.MinText = "-7";
        catalog.SelectedAffix.MaxText = "-3";

        Assert.Equal(
            "items.json: item 'fierce_blade': fixedAffixes[0] (fierce): value must be below 0, as affix 'fierce' rolls (was 5).",
            catalog.Problem);
    }

    [Fact]
    public void Problem_TwoNewAffixesWithTheSameId_NamesIt()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        AddWard(catalog, "ward");

        AddWard(catalog, "ward");

        Assert.Equal("Affix ward is listed twice.", catalog.Problem);
    }

    // Set up against list order: the earlier affix's problem would come first if the chosen one's weren't read first.
    [Fact]
    public void Problem_ChosenAffixHasAProblem_IsItsOwnFirst()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        AddWard(catalog, "ward").MinText = "abc";

        AddWard(catalog, "guard").MaxText = "xyz";

        Assert.Equal("Max must be a whole number (was 'xyz').", catalog.Problem);
    }

    [Fact]
    public void Problem_KindTicked_IsRaised()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        catalog.SelectedAffix = AffixOf(catalog, "fireproof");
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        AffixFixtures.Toggle(catalog.SelectedAffix, EquipmentKind.Head).IsChosen = true;

        Assert.Contains(nameof(AffixCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void Problem_RemovedAffixTyped_IsNotRaised()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);
        var removed = AddWard(catalog, "ward");
        catalog.RemoveAffixCommand.Execute(null);
        var raised = new List<string?>();
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        removed.MaxText = "4";

        Assert.Empty(raised);
    }

    [Fact]
    public void ToAffixes_Problem_Throws()
    {
        var catalog = new AffixCatalogViewModel(AffixFixtures.Catalog);

        catalog.AddAffixCommand.Execute(null);

        Assert.Throws<InvalidOperationException>(catalog.ToAffixes);
    }

    private static AffixViewModel AffixOf(AffixCatalogViewModel catalog, string id) =>
        catalog.Affixes.Single(affix => affix.Id == id);

    // A new affix with the id, rolling on the head, so Data takes it.
    private static AffixViewModel AddWard(AffixCatalogViewModel catalog, string id)
    {
        catalog.AddAffixCommand.Execute(null);
        var affix = catalog.SelectedAffix!;
        affix.Id = id;
        AffixFixtures.Toggle(affix, EquipmentKind.Head).IsChosen = true;
        return affix;
    }
}
