using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// A unique's fixed affix rows through the Items list's problem line (decision 866): Data's bound, its refusal of 0, of
// the wrong side of zero and of an affix named twice, the roll range left free, re-checked as a row is typed.
public sealed class ItemCatalogViewModelUniqueTests
{
    // An affix rolling below zero, which the test world's pool lacks, for the sign rule's other side.
    private static readonly AffixDefinition _clumsy = new()
    {
        Id = "clumsy",
        Stat = StatKind.Agility,
        Min = -3,
        Max = -1,
        Kinds = [EquipmentKind.Ring],
        Weight = 10,
        GoldValue = 5,
    };

    private static readonly ContentCatalog _withClumsy = ItemFixtures.Catalog with
    {
        Affixes = ItemFixtures.Catalog.Affixes.Values.Append(_clumsy).ToDictionary(affix => affix.Id),
    };

    // power rolls 2..5: 9 is past its range and still taken (decision 866), as are both ends of ±100's positive side.
    [Theory]
    [InlineData("1")]
    [InlineData("9")]
    [InlineData("100")]
    public void Problem_FixedValueWithinTheBoundOnItsSide_IsNull(string value)
    {
        var catalog = WithWarlordAxeChosen(ItemFixtures.Catalog);

        catalog.SelectedItem!.Unique!.Rows[0].ValueText = value;

        Assert.Null(catalog.Problem);
    }

    [Theory]
    [InlineData("101")]
    [InlineData("-101")]
    public void Problem_FixedValuePastTheBound_IsDatasMessage(string value)
    {
        var catalog = WithWarlordAxeChosen(ItemFixtures.Catalog);

        catalog.SelectedItem!.Unique!.Rows[0].ValueText = value;

        Assert.Equal(
            $"items.json: item 'warlord_axe': fixedAffixes[0] (power): value must be nonzero and within -100..100 (was {value}).",
            catalog.Problem);
    }

    [Fact]
    public void Problem_FixedValueOfZero_IsDatasMessage()
    {
        var catalog = WithWarlordAxeChosen(ItemFixtures.Catalog);

        catalog.SelectedItem!.Unique!.Rows[0].ValueText = "0";

        Assert.Equal(
            "items.json: item 'warlord_axe': fixedAffixes[0] (power): value must be nonzero and within -100..100 (was 0).",
            catalog.Problem);
    }

    [Fact]
    public void Problem_FixedValueBelowZeroOnARisingAffix_IsDatasMessage()
    {
        var catalog = WithWarlordAxeChosen(ItemFixtures.Catalog);

        catalog.SelectedItem!.Unique!.Rows[0].ValueText = "-5";

        Assert.Equal(
            "items.json: item 'warlord_axe': fixedAffixes[0] (power): value must be above 0, as affix 'power' rolls (was -5).",
            catalog.Problem);
    }

    [Fact]
    public void Problem_FixedValueAtTheLowerBoundOnAFallingAffix_IsNull()
    {
        var catalog = WithClumsyFirst();

        catalog.SelectedItem!.Unique!.Rows[0].ValueText = "-100";

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void Problem_FixedValueAboveZeroOnAFallingAffix_IsDatasMessage()
    {
        var catalog = WithClumsyFirst();

        catalog.SelectedItem!.Unique!.Rows[0].ValueText = "5";

        Assert.Equal(
            "items.json: item 'warlord_axe': fixedAffixes[0] (clumsy): value must be below 0, as affix 'clumsy' rolls (was 5).",
            catalog.Problem);
    }

    [Fact]
    public void Problem_AffixNamedTwice_IsDatasMessage()
    {
        var catalog = WithWarlordAxeChosen(ItemFixtures.Catalog);
        var unique = catalog.SelectedItem!.Unique!;

        unique.Rows[1].Affix = unique.Rows[0].Affix;

        Assert.Equal("items.json: item 'warlord_axe': fixedAffixes names 'power' twice.", catalog.Problem);
    }

    [Fact]
    public void Problem_FixedValueTyped_IsRaised()
    {
        var catalog = WithWarlordAxeChosen(ItemFixtures.Catalog);
        var raised = 0;
        catalog.PropertyChanged += (_, e) => raised += e.PropertyName == nameof(ItemCatalogViewModel.Problem) ? 1 : 0;

        catalog.SelectedItem!.Unique!.Rows[0].ValueText = "0";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void AddItem_Unique_OffersTheDraftsAffixes()
    {
        var catalog = new ItemCatalogViewModel(_withClumsy, [], []);

        catalog.AddItemCommand.Execute(ItemCategory.Unique);

        Assert.Contains("clumsy", catalog.SelectedItem!.Unique!.AffixChoices.Select(choice => choice.Id));
    }

    private static ItemCatalogViewModel WithWarlordAxeChosen(ContentCatalog content)
    {
        var catalog = new ItemCatalogViewModel(content, [], []);
        catalog.SelectedItem = catalog.Items.Single(item => item.Id == "warlord_axe");
        return catalog;
    }

    // The axe's first row names the falling affix at a value on its side, so only the value typed decides the line.
    private static ItemCatalogViewModel WithClumsyFirst()
    {
        var catalog = WithWarlordAxeChosen(_withClumsy);
        var row = catalog.SelectedItem!.Unique!.Rows[0];
        row.Affix = catalog.SelectedItem.Unique.AffixChoices.Single(choice => choice.Id == "clumsy");
        row.ValueText = "-2";
        return catalog;
    }
}
