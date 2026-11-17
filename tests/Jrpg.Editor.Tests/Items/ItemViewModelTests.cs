using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// One item's general fields as the Items page types them, written over the record they came from; the sale line read
// from Core's formula at the test's own share.
public sealed class ItemViewModelTests
{
    private const int SellPercent = 50;

    // Each typed box's edit, a value away from the tonic's.
    private static readonly Dictionary<string, Action<ItemViewModel>> _typings = new()
    {
        [nameof(ItemViewModel.Id)] = item => item.Id = "draught",
        [nameof(ItemViewModel.Name)] = item => item.Name = "Draught",
        [nameof(ItemViewModel.Description)] = item => item.Description = "Restores some HP.",
        [nameof(ItemViewModel.IconFile)] = item => item.IconFile = "Draught.png",
        [nameof(ItemViewModel.MaxStackText)] = item => item.MaxStackText = "9",
        [nameof(ItemViewModel.PriceText)] = item => item.PriceText = "45",
        [nameof(ItemViewModel.IsUsableFromMenu)] = item => item.IsUsableFromMenu = false,
    };

    [Fact]
    public void From_Weapon_ShowsItsFieldsAndHidesMaxStack()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Equal(("copper_axe", "Copper Axe", "A heavy axe."), (item.Id, item.Name, item.Description));
        Assert.Equal(("Axe.png", "99", "100"), (item.IconFile, item.MaxStackText, item.PriceText));
        Assert.Equal(ItemCategory.Equipment, item.Category);
        Assert.False(item.ShowsMaxStack);
        Assert.False(item.IsNew);
        Assert.Null(item.Problem);
    }

    [Theory]
    [InlineData("tonic", true, true)]
    [InlineData("ore", true, false)]
    [InlineData("copper_vest", false, false)]
    [InlineData("warlord_axe", false, false)]
    public void ShowsBoxes_EachCategory_MatchesItsFields(string id, bool showsMaxStack, bool showsUsableFromMenu)
    {
        var item = ItemViewModel.From(ItemFixtures.Catalog.Items[id], SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Equal((showsMaxStack, showsUsableFromMenu), (item.ShowsMaxStack, item.ShowsUsableFromMenu));
    }

    [Fact]
    public void ToItem_Untouched_EqualsTheSource()
    {
        var item = ItemViewModel.From(ItemFixtures.Tonic, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Equal(ItemFixtures.Tonic, item.ToItem());
    }

    [Fact]
    public void ToItem_GeneralFieldsTyped_WritesEachOverTheSource()
    {
        var item = ItemViewModel.From(ItemFixtures.Tonic, SellPercent, ItemFixtures.Catalog.Affixes);

        item.Name = " Draught ";
        item.Description = "Restores some HP.";
        item.IconFile = "Draught.png";
        item.MaxStackText = "9";
        item.IsUsableFromMenu = false;

        var expected = ItemFixtures.Tonic with
        {
            Name = "Draught",
            Description = "Restores some HP.",
            IconFile = "Draught.png",
            MaxStack = 9,
            IsUsableFromMenu = false,
        };
        Assert.Equal(expected, item.ToItem());
    }

    [Fact]
    public void ToItem_NameAndPriceTyped_CarriesTheEquipmentUntouched()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        item.Name = "Bronze Axe";
        item.PriceText = "120";

        var built = item.ToItem();
        Assert.Equal(("Bronze Axe", 120), (built.Name, built.Price));
        Assert.Equal(ItemFixtures.CopperAxe.Equipment, built.Equipment);
    }

    [Fact]
    public void ToItem_UniqueTyped_CarriesItsFixedAffixes()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        item.Name = "Tyrant's Axe";

        var built = item.ToItem();
        Assert.True(built.IsUnique);
        Assert.Equal(ItemFixtures.WarlordAxe.FixedAffixes, built.FixedAffixes);
    }

    [Fact]
    public void ToItem_PriceEmpty_HasNoPrice()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperVest, SellPercent, ItemFixtures.Catalog.Affixes);

        item.PriceText = " ";

        Assert.Null(item.ToItem().Price);
    }

    [Fact]
    public void ToItem_PriceZero_IsPricedZero()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperVest, SellPercent, ItemFixtures.Catalog.Affixes);

        item.PriceText = "0";

        Assert.Equal(0, item.ToItem().Price);
    }

    [Fact]
    public void ToItem_WithAProblem_Throws()
    {
        var item = ItemViewModel.From(ItemFixtures.Ore, SellPercent, ItemFixtures.Catalog.Affixes);

        item.MaxStackText = "many";

        Assert.Throws<InvalidOperationException>(item.ToItem);
    }

    [Fact]
    public void SaleLine_Price100AtFiftyPercent_SaysFifty()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Equal("Sells for 50 (50 % share)", item.SaleLine);
    }

    [Fact]
    public void SaleLine_PriceCleared_SaysNeverTraded()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        item.PriceText = "";

        Assert.Equal("Never bought nor sold", item.SaleLine);
    }

    [Fact]
    public void SaleLine_PriceNotANumber_IsEmpty()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        item.PriceText = "cheap";

        Assert.Equal("", item.SaleLine);
    }

    [Fact]
    public void SaleLine_PriceTyped_RaisesItsChange()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperAxe, SellPercent, ItemFixtures.Catalog.Affixes);
        var raised = new List<string?>();
        item.PropertyChanged += (_, args) => raised.Add(args.PropertyName);

        item.PriceText = "60";

        Assert.Contains(nameof(ItemViewModel.SaleLine), raised);
        Assert.Equal("Sells for 30 (50 % share)", item.SaleLine);
    }

    [Fact]
    public void Problem_MaxStackNotANumber_NamesTheBox()
    {
        var item = ItemViewModel.From(ItemFixtures.Ore, SellPercent, ItemFixtures.Catalog.Affixes);

        item.MaxStackText = "many";

        Assert.Equal("Max stack must be a whole number (was 'many').", item.Problem);
    }

    [Fact]
    public void Problem_PriceNotANumber_NamesTheBox()
    {
        var item = ItemViewModel.From(ItemFixtures.Ore, SellPercent, ItemFixtures.Catalog.Affixes);

        item.PriceText = "cheap";

        Assert.Equal("Price must be a whole number or empty (was 'cheap').", item.Problem);
    }

    [Fact]
    public void Problem_NewWithoutId_NamesTheId()
    {
        var item = ItemViewModel.New(ItemFixtures.Ore, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Equal("The id must not be empty.", item.Problem);
    }

    [Fact]
    public void Problem_SavedItem_IgnoresTheIdRule()
    {
        var item = ItemViewModel.From(ItemFixtures.Ore with { Id = "Old_Ore" }, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Null(item.Problem);
    }

    [Fact]
    public void Label_SavedUnique_NamesItsCategoryNameAndId()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Equal("Unique · Warlord's Axe (warlord_axe)", item.Label);
    }

    [Fact]
    public void Label_NewWithoutId_NamesItsCategory()
    {
        var item = ItemViewModel.New(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        Assert.Equal("(new unique)", item.Label);
    }

    [Fact]
    public void Duplicate_Unique_IsANewUniqueWithEveryFieldButTheId()
    {
        var item = ItemViewModel.From(ItemFixtures.WarlordAxe, SellPercent, ItemFixtures.Catalog.Affixes);

        var copy = item.Duplicate();
        copy.Id = "warlord_axe_2";

        Assert.True(copy.IsNew);
        Assert.Equal(ItemCategory.Unique, copy.Category);
        Assert.Equal(ItemFixtures.WarlordAxe with { Id = "warlord_axe_2" }, copy.ToItem());
    }

    [Fact]
    public void Duplicate_HalfTypedPrice_CopiesItAsTyped()
    {
        var item = ItemViewModel.From(ItemFixtures.CopperVest, SellPercent, ItemFixtures.Catalog.Affixes);
        item.PriceText = "8x";

        var copy = item.Duplicate();

        Assert.Equal(("", "8x"), (copy.Id, copy.PriceText));
    }

    [Theory]
    [InlineData(nameof(ItemViewModel.Id))]
    [InlineData(nameof(ItemViewModel.Name))]
    [InlineData(nameof(ItemViewModel.Description))]
    [InlineData(nameof(ItemViewModel.IconFile))]
    [InlineData(nameof(ItemViewModel.MaxStackText))]
    [InlineData(nameof(ItemViewModel.PriceText))]
    [InlineData(nameof(ItemViewModel.IsUsableFromMenu))]
    public void Changed_TypedBoxEdited_IsRaised(string box)
    {
        var item = ItemViewModel.New(ItemFixtures.Tonic, SellPercent, ItemFixtures.Catalog.Affixes);
        var raised = 0;
        item.Changed += (_, _) => raised++;

        _typings[box](item);

        Assert.Equal(1, raised);
    }
}
