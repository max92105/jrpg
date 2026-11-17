using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// A unique's fixed affix form (decision 882) as the item holds it: set for uniques only, its problem after the
// equipment's, its rows in the item's record, its typed rows relayed and copied by Duplicate, and the piece's price
// line following the price box and the rows (decision 871).
public sealed class ItemViewModelUniqueTests
{
    private const int SellPercent = 50;

    [Fact]
    public void Unique_OfAPlainSword_IsNull()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Null(item.Unique);
        Assert.Equal("", item.PieceValueLine);
    }

    [Fact]
    public void Unique_OfAUnique_ListsItsFixedAffixes()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Equal(["power", "crit_chance"], item.Unique?.Rows.Select(row => row.Affix.Id));
    }

    [Fact]
    public void Unique_NewUnique_HasNoRowAndOffersThePool()
    {
        var item = ItemViewModel.New(
            ItemCategories.StartingRecord(ItemCategory.Unique), SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Empty(item.Unique!.Rows);
        Assert.Equal(ItemFixtures.Catalog.Affixes.Count, item.Unique.AffixChoices.Count);
    }

    // The warlord axe costs 300 and its two affixes add 20 each in the test world: 340, then 440 at 400.
    [Fact]
    public void PieceValueLine_PriceTyped_IsRecomputed()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);
        Assert.Equal("Price as a piece: 340, sells for 170", item.PieceValueLine);
        var changed = new List<string?>();
        item.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        item.PriceText = "400";

        Assert.Contains(nameof(ItemViewModel.PieceValueLine), changed);
        Assert.Equal("Price as a piece: 440, sells for 220", item.PieceValueLine);
    }

    [Fact]
    public void PieceValueLine_RowRemoved_IsRaised()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);
        var changed = new List<string?>();
        item.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        item.Unique!.RemoveRowCommand.Execute(null);

        Assert.Equal([nameof(ItemViewModel.PieceValueLine)], changed);
        Assert.Equal("Price as a piece: 320, sells for 160", item.PieceValueLine);
    }

    [Fact]
    public void ToItem_FixedValueTyped_TakesIt()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        item.Unique!.Rows[1].ValueText = "9";

        RolledAffix[] expected = [new("power", StatKind.Power, 6), new("crit_chance", StatKind.CritChance, 9)];
        Assert.Equal(expected, item.ToItem().FixedAffixes);
    }

    [Fact]
    public void Problem_FixedValueNotANumber_IsTheItems()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        item.Unique!.Rows[0].ValueText = "lots";

        Assert.Equal("fixedAffixes[0] (power): Value must be a whole number (was 'lots').", item.Problem);
    }

    [Fact]
    public void Problem_DamageAndFixedValueNotNumbers_NamesTheDamageFirst()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);
        item.Unique!.Rows[0].ValueText = "lots";

        item.Equipment!.DamageText = "big";

        Assert.Equal("Damage must be a whole number (was 'big').", item.Problem);
    }

    [Fact]
    public void Changed_FixedValueTyped_IsRaised()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);
        var raised = 0;
        item.Changed += (_, _) => raised++;

        item.Unique!.Rows[0].ValueText = "7";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Duplicate_HalfTypedFixedValue_CopiesTheRowsAsTyped()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);
        item.Unique!.Rows[1].ValueText = "3x";

        var copy = item.Duplicate();

        Assert.Equal(["6", "3x"], copy.Unique?.Rows.Select(row => row.ValueText));
        Assert.NotSame(item.Unique, copy.Unique);
    }

    [Fact]
    public void Duplicate_CopyRowAdded_RaisesTheCopysChangedAndLine()
    {
        var copy = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes).Duplicate();
        var raised = 0;
        var changed = new List<string?>();
        copy.Changed += (_, _) => raised++;
        copy.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        copy.Unique!.AddRowCommand.Execute(null);

        Assert.Equal(1, raised);
        Assert.Equal([nameof(ItemViewModel.PieceValueLine)], changed);
    }
}
