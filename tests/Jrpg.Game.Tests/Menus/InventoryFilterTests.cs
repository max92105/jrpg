using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Data.Content;
using Jrpg.Game.Menus;

namespace Jrpg.Game.Tests.Menus;

public sealed class InventoryFilterTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    // Picked up out of category order, so a test can see the order kept.
    private static readonly IReadOnlyList<InventoryEntry> _owned =
    [
        new("leather_armor", 1),
        new("potion", 4),
        new("bronze_sword", 2),
        new("wizard_robe", 1),
        new("iron_sword", 1),
        new("bomb", 3),
    ];

    [Fact]
    public void Select_Weapons_KeepsOnlyWeaponKindItems()
    {
        var kept = InventoryFilter.Select(InventoryCategory.Weapons, _owned, _catalog.Items);

        Assert.Equal(["bronze_sword", "iron_sword"], kept.Select(entry => entry.ItemId));
    }

    [Fact]
    public void Select_Consumables_KeepsItemsWithEffect()
    {
        var kept = InventoryFilter.Select(InventoryCategory.Consumables, _owned, _catalog.Items);

        Assert.Equal(["potion", "bomb"], kept.Select(entry => entry.ItemId));
    }

    [Fact]
    public void Select_Armor_KeepsEveryPieceThatIsNotAWeapon()
    {
        var kept = InventoryFilter.Select(InventoryCategory.Armor, _owned, _catalog.Items);

        Assert.Equal(["leather_armor", "wizard_robe"], kept.Select(entry => entry.ItemId));
    }

    [Fact]
    public void Select_All_KeepsPickupOrder()
    {
        var kept = InventoryFilter.Select(InventoryCategory.All, _owned, _catalog.Items);

        Assert.Equal(_owned, kept);
    }
}
