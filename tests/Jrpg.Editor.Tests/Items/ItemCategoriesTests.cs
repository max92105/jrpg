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
}
