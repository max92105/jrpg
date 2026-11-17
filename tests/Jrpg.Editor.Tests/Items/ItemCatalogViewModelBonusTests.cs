using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// An equipment's bonus rows through the Items list's problem line (decision 880): Data's bound and its refusal of 0,
// and the form's own stat named twice, re-checked as a row is typed.
public sealed class ItemCatalogViewModelBonusTests
{
    [Theory]
    [InlineData("100")]
    [InlineData("-100")]
    public void Problem_BonusAtTheBound_IsNull(string value)
    {
        var catalog = WithVestChosen();

        catalog.SelectedItem!.Equipment!.Bonuses[0].ValueText = value;

        Assert.Null(catalog.Problem);
    }

    [Theory]
    [InlineData("101")]
    [InlineData("-101")]
    [InlineData("0")]
    public void Problem_BonusPastTheBoundOrZero_IsDatasMessage(string value)
    {
        var catalog = WithVestChosen();

        catalog.SelectedItem!.Equipment!.Bonuses[0].ValueText = value;

        Assert.Equal(
            $"items.json: item 'copper_vest': equipment.bonuses.constitution must be nonzero and within -100..100 (was {value}).",
            catalog.Problem);
    }

    // The record's bonuses can't hold a stat twice, so Data never sees the second row: the form names it.
    [Fact]
    public void Problem_SecondRowWithTheSameStat_IsTheFormsMessage()
    {
        var catalog = WithVestChosen();
        var equipment = catalog.SelectedItem!.Equipment!;
        equipment.AddBonusCommand.Execute(null);

        equipment.Bonuses[1].Stat = equipment.Bonuses[0].Stat;

        Assert.Equal("bonuses names constitution twice.", catalog.Problem);
    }

    private static ItemCatalogViewModel WithVestChosen()
    {
        var catalog = new ItemCatalogViewModel(ItemFixtures.Catalog, [], []);
        catalog.SelectedItem = catalog.Items.Single(item => item.Id == "copper_vest");
        return catalog;
    }
}
