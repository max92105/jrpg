using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// An item's equipment form (decision 880) as the item holds it: set for equipment and uniques, its problem after the
// general boxes', its record in the item's, its typed boxes relayed and copied by Duplicate.
public sealed class ItemViewModelEquipmentTests
{
    private const int SellPercent = 50;

    [Theory]
    [InlineData("tonic", false)]
    [InlineData("ore", false)]
    [InlineData("copper_vest", true)]
    [InlineData("warlord_axe", true)]
    public void Equipment_EachCategory_SetForEquipmentAndUniques(string id, bool hasEquipment)
    {
        var item = ItemViewModel.From(ItemFixtures.Catalog.Items[id], SellPercent);

        Assert.Equal(hasEquipment, item.Equipment is not null);
    }

    [Fact]
    public void ToItem_UntouchedUnique_EqualsTheSource()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent);

        Assert.Equal(ItemFixtures.WarlordAxe, item.ToItem());
    }

    [Fact]
    public void ToItem_EquipmentEdited_TakesTheForm()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent);

        item.Equipment!.DamageText = "24";
        item.Equipment.DamageType = DamageType.Blunt;

        var expected = ItemFixtures.CopperAxe.Equipment! with { Damage = 24, DamageType = DamageType.Blunt };
        Assert.Equal(expected, item.ToItem().Equipment);
    }

    [Fact]
    public void Problem_WeaponDamageNotANumber_IsTheItems()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent);

        item.Equipment!.DamageText = "big";

        Assert.Equal("Damage must be a whole number (was 'big').", item.Problem);
    }

    [Fact]
    public void Problem_PriceAndDamageNotNumbers_NamesThePriceFirst()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent);
        item.Equipment!.DamageText = "big";

        item.PriceText = "cheap";

        Assert.Equal("Price must be a whole number or empty (was 'cheap').", item.Problem);
    }

    [Fact]
    public void Changed_EquipmentKindPicked_IsRaised()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperVest, SellPercent);
        var raised = 0;
        item.Changed += (_, _) => raised++;

        item.Equipment!.Kind = EquipmentKind.Feet;

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Duplicate_HalfTypedDamage_CopiesTheFormAsTyped()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent);
        item.Equipment!.DamageText = "2x";

        var copy = item.Duplicate();

        Assert.Equal("2x", copy.Equipment?.DamageText);
        Assert.NotSame(item.Equipment, copy.Equipment);
    }

    [Fact]
    public void Duplicate_CopyTyped_RaisesTheCopysChanged()
    {
        var copy = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent).Duplicate();
        var raised = 0;
        copy.Changed += (_, _) => raised++;

        copy.Equipment!.DamageText = "30";

        Assert.Equal(1, raised);
    }
}
