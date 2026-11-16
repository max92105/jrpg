using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

public sealed class ItemCategoriesTests
{
    [Fact]
    public void Of_Material_IsMaterial()
    {
        var category = ItemCategories.Of(ItemFixtures.Ore);

        Assert.Equal(ItemCategory.Material, category);
    }

    [Fact]
    public void Of_Weapon_IsEquipment()
    {
        var category = ItemCategories.Of(ItemFixtures.CopperAxe);

        Assert.Equal(ItemCategory.Equipment, category);
    }

    [Fact]
    public void Of_UniqueWeapon_IsUnique()
    {
        var category = ItemCategories.Of(ItemFixtures.WarlordAxe);

        Assert.Equal(ItemCategory.Unique, category);
    }

    [Fact]
    public void Of_Potion_IsConsumable()
    {
        var category = ItemCategories.Of(ItemFixtures.Tonic);

        Assert.Equal(ItemCategory.Consumable, category);
    }

    // Each Add button's record is read back as the category it names, so the page shows its new item's forms.
    [Fact]
    public void StartingRecord_EachCategory_IsOfThatCategory()
    {
        var categories = Enum.GetValues<ItemCategory>();

        var readBack = categories.Select(category => ItemCategories.Of(ItemCategories.StartingRecord(category)));

        Assert.Equal(categories, readBack);
    }
}
