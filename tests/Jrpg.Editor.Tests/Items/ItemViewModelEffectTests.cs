using Jrpg.Core.Effects;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// A consumable's effect form and own strip (decision 881) as the item holds them: the form set for consumables only,
// its problem after the general boxes', its record and the strip in the item's, its boxes relayed and duplicated.
public sealed class ItemViewModelEffectTests
{
    private const int SellPercent = 50;

    [Theory]
    [InlineData("tonic", true)]
    [InlineData("ore", false)]
    [InlineData("copper_vest", false)]
    [InlineData("warlord_axe", false)]
    public void Effect_EachCategory_SetForConsumablesOnly(string id, bool hasEffect)
    {
        var item = ItemViewModel.From(ItemFixtures.Catalog.Items[id], SellPercent);

        Assert.Equal(hasEffect, item.Effect is not null);
    }

    [Fact]
    public void ToItem_UntouchedBomb_EqualsTheSource()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);

        Assert.Equal(ItemFixtures.Firebomb, item.ToItem());
    }

    // The power box is hidden for a rest, so a hand-written power nothing reads stays as the file holds it.
    [Fact]
    public void ToItem_UntouchedRestWithAHiddenPower_EqualsTheSource()
    {
        var tent = ItemFixtures.Tonic with { Id = "tent", Effect = new Effect(EffectKind.Rest, 5, Targeting.AllAllies) };
        var item = ItemViewModel.From(tent, SellPercent);

        Assert.Equal(tent, item.ToItem());
    }

    [Fact]
    public void ToItem_EffectEdited_TakesTheForm()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);

        item.Effect!.PowerText = "18";
        item.Effect.Targeting = Targeting.SingleEnemy;

        var expected = ItemFixtures.Firebomb.Effect! with { Power = 18, Targeting = Targeting.SingleEnemy };
        Assert.Equal(expected, item.ToItem().Effect);
    }

    [Fact]
    public void ToItem_StripCleared_HasNoEffectFile()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);

        item.EffectFile = " ";

        Assert.Null(item.ToItem().EffectFile);
    }

    [Fact]
    public void ToItem_StripTyped_TakesItTrimmed()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);

        item.EffectFile = " Flame.png ";

        Assert.Equal("Flame.png", item.ToItem().EffectFile);
    }

    [Fact]
    public void Problem_PowerNotANumber_IsTheItems()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);

        item.Effect!.PowerText = "big";

        Assert.Equal("Power must be a whole number (was 'big').", item.Problem);
    }

    [Fact]
    public void Problem_PriceAndPowerNotNumbers_NamesThePriceFirst()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);
        item.Effect!.PowerText = "big";

        item.PriceText = "cheap";

        Assert.Equal("Price must be a whole number or empty (was 'cheap').", item.Problem);
    }

    [Fact]
    public void Changed_EffectTargetingPicked_IsRaised()
    {
        var item = ItemViewModel.From(ItemFixtures.Tonic, SellPercent);
        var raised = 0;
        item.Changed += (_, _) => raised++;

        item.Effect!.Targeting = Targeting.AllAllies;

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_StripTyped_IsRaised()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);
        var raised = 0;
        item.Changed += (_, _) => raised++;

        item.EffectFile = "Flame.png";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Duplicate_HalfTypedPower_CopiesTheFormAsTyped()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);
        item.Effect!.PowerText = "2x";

        var copy = item.Duplicate();

        Assert.Equal("2x", copy.Effect?.PowerText);
        Assert.NotSame(item.Effect, copy.Effect);
    }

    [Fact]
    public void Duplicate_StripTyped_CopiesIt()
    {
        var item = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent);
        item.EffectFile = "Flame.png";

        var copy = item.Duplicate();

        Assert.Equal("Flame.png", copy.EffectFile);
    }

    [Fact]
    public void Duplicate_CopyTyped_RaisesTheCopysChanged()
    {
        var copy = ItemViewModel.From(ItemFixtures.Firebomb, SellPercent).Duplicate();
        var raised = 0;
        copy.Changed += (_, _) => raised++;

        copy.Effect!.PowerText = "30";

        Assert.Equal(1, raised);
    }
}
